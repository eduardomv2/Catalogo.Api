using Catalogo.Api.Data;
using Catalogo.Api.DTOs;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios ─────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Catalogo API",
        Version = "v1",
        Description = "Microservicio de gestión de productos y categorías"
    });
});

builder.Services.AddDbContext<CatalogoDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration
        .GetConnectionString("CatalogoDb")));

var app = builder.Build();

// ── Middleware ───
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var error = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            status = 500,
            error = "Error interno del servidor.",
            detalle = error?.Error.Message,
            timestamp = DateTime.UtcNow
        });
    });
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalogo API v1");
    c.RoutePrefix = "swagger";
});

// ── Migración automática ──────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogoDbContext>();
    db.Database.Migrate();
}

// ══════════════════════════════════════════════════════════════════
// ENDPOINTS
// ══════════════════════════════════════════════════════════════════

// GET /health
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Catalogo API",
    timestamp = DateTime.UtcNow
}))
.WithName("Health")
.WithTags("Health")
.WithSummary("Verifica estado del microservicio");

// ── Categorías ────────────────────────────────────────────────────

// GET /api/catalogo/categorias
app.MapGet("/api/catalogo/categorias", async (CatalogoDbContext db) =>
    Results.Ok(await db.Categorias
        .Where(c => c.Status == 1)
        .ToListAsync()))
.WithName("ObtenerCategorias")
.WithTags("Categorias")
.WithSummary("Lista todas las categorías");

// POST /api/catalogo/categorias
app.MapPost("/api/catalogo/categorias", async (
    CrearCategoriaDto dto,
    CatalogoDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return Results.BadRequest(new
        { error = "El nombre de la categoría es obligatorio." });

    var categoria = new CAT_Categoria
    {
        Nombre = dto.Nombre,
        EsElectronica = dto.EsElectronica,
        Status = 1
    };

    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/catalogo/categorias/{categoria.Id}",
        categoria);
})
.WithName("CrearCategoria")
.WithTags("Categorias")
.WithSummary("Crea una nueva categoría");

// ── Marcas ────────────────────────────────────────────────────────

// GET /api/catalogo/marcas
app.MapGet("/api/catalogo/marcas", async (CatalogoDbContext db) =>
    Results.Ok(await db.Marcas
        .Where(m => m.Status == 1)
        .ToListAsync()))
.WithName("ObtenerMarcas")
.WithTags("Marcas")
.WithSummary("Lista todas las marcas");

// POST /api/catalogo/marcas
app.MapPost("/api/catalogo/marcas", async (
    CrearMarcaDto dto,
    CatalogoDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return Results.BadRequest(new
        { error = "El nombre de la marca es obligatorio." });

    var marca = new CAT_Marca
    {
        Nombre = dto.Nombre,
        Status = 1
    };

    db.Marcas.Add(marca);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/catalogo/marcas/{marca.Id}",
        marca);
})
.WithName("CrearMarca")
.WithTags("Marcas")
.WithSummary("Crea una nueva marca");

// ── Productos ──────

// GET /api/catalogo/productos
app.MapGet("/api/catalogo/productos", async (
    CatalogoDbContext db,
    int pagina = 1,
    int cantidad = 8) =>
{
    var query = db.Productos
        .Include(p => p.Categoria)
        .Include(p => p.Marca)
        .Include(p => p.Imagenes)
        .Where(p => p.Status == 1);

    var total = await query.CountAsync();

    var productos = await query
        .OrderBy(p => p.Id)
        .Skip((pagina - 1) * cantidad)
        .Take(cantidad)
        .Select(p => new
        {
            p.Id,
            p.Nombre,
            p.Descripcion,
            p.PrecioVenta,
            p.Stock,
            Categoria = p.Categoria.Nombre,
            EsElectronica = p.Categoria.EsElectronica,
            Marca = p.Marca.Nombre,
            ImagenPortada = p.Imagenes
                .Where(i => i.EsPortada && i.Status == 1)
                .Select(i => i.UrlImagen)
                .FirstOrDefault()
        })
        .ToListAsync();

    return Results.Ok(new
    {
        productos,
        total,
        pagina,
        cantidad,
        hayMas = pagina * cantidad < total
    });
});

// GET /api/catalogo/productos/{id}
app.MapGet("/api/catalogo/productos/{id:int}", async (
    int id,
    CatalogoDbContext db) =>
{
    var producto = await db.Productos
        .Include(p => p.Categoria)
        .Include(p => p.Marca)
        .Include(p => p.Imagenes.Where(i => i.Status == 1))
        .FirstOrDefaultAsync(p => p.Id == id && p.Status == 1);

    return producto is null
        ? Results.NotFound(new { error = "Producto no encontrado." })
        : Results.Ok(new
        {
            producto.Id,
            producto.Nombre,
            producto.Descripcion,
            producto.PrecioVenta,
            producto.Stock,
            Categoria = new
            {
                producto.Categoria.Id,
                producto.Categoria.Nombre,
                producto.Categoria.EsElectronica
            },
            Marca = new
            {
                producto.Marca.Id,
                producto.Marca.Nombre
            },
            Imagenes = producto.Imagenes.Select(i => new
            {
                i.Id,
                i.UrlImagen,
                i.EsPortada
            }),
            Descuento = CalculadorDescuento.CalcularConDetalle(
                producto.PrecioVenta,
                producto.Categoria.EsElectronica)
        });
})
.WithName("ObtenerProductoPorId")
.WithTags("Productos")
.WithSummary("Obtiene un producto por Id con descuento calculado");

// POST /api/catalogo/productos
app.MapPost("/api/catalogo/productos", async (
    CrearProductoDto dto,
    CatalogoDbContext db) =>
{
    var productoTemp = new CAT_Producto
    {
        Nombre = dto.Nombre,
        Descripcion = dto.Descripcion,
        PrecioVenta = dto.PrecioVenta,
        Stock = dto.Stock,
        IdCategoria = dto.IdCategoria,
        IdMarca = dto.IdMarca
    };

    var errores = ValidadorProducto.ObtenerErrores(productoTemp);
    if (errores.Any())
        return Results.BadRequest(new { errores });

    var categoriaExiste = await db.Categorias
        .AnyAsync(c => c.Id == dto.IdCategoria && c.Status == 1);
    if (!categoriaExiste)
        return Results.NotFound(new
        { error = "La categoría especificada no existe." });

    var marcaExiste = await db.Marcas
        .AnyAsync(m => m.Id == dto.IdMarca && m.Status == 1);
    if (!marcaExiste)
        return Results.NotFound(new
        { error = "La marca especificada no existe." });

    productoTemp.Status = 1;
    db.Productos.Add(productoTemp);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/catalogo/productos/{productoTemp.Id}",
        new { productoTemp.Id, productoTemp.Nombre, productoTemp.PrecioVenta });
})
.WithName("CrearProducto")
.WithTags("Productos")
.WithSummary("Crea un nuevo producto");

// DELETE /api/catalogo/productos/{id}
app.MapDelete("/api/catalogo/productos/{id:int}", async (
    int id,
    CatalogoDbContext db) =>
{
    var producto = await db.Productos
        .FirstOrDefaultAsync(p => p.Id == id && p.Status == 1);

    if (producto is null)
        return Results.NotFound(new { error = "Producto no encontrado." });

    producto.Status = 0; // borrado lógico
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("EliminarProducto")
.WithTags("Productos")
.WithSummary("Elimina un producto (borrado lógico)");

// PATCH /api/catalogo/productos/{id}/stock
app.MapMethods("/api/catalogo/productos/{id:int}/stock", ["PATCH"], async (
    int id,
    ActualizarStockDto dto,
    CatalogoDbContext db) =>
{
    var producto = await db.Productos
        .FirstOrDefaultAsync(p => p.Id == id && p.Status == 1);

    if (producto is null)
        return Results.NotFound(new { error = "Producto no encontrado." });

    if (producto.Stock < dto.Cantidad)
        return Results.BadRequest(new
        { error = $"Stock insuficiente. Disponible: {producto.Stock}" });

    producto.Stock -= dto.Cantidad;
    await db.SaveChangesAsync();

    return Results.Ok(new { producto.Id, producto.Stock });
})
.WithName("ActualizarStock")
.WithTags("Catalogo")
.WithSummary("Descuenta stock de un producto al comprarlo");

// ── Imágenes ───

// POST /api/catalogo/productos/{id}/imagenes
app.MapPost("/api/catalogo/productos/{id:int}/imagenes", async (
    int id,
    AgregarImagenDto dto,
    CatalogoDbContext db) =>
{
    var productoExiste = await db.Productos
        .AnyAsync(p => p.Id == id && p.Status == 1);
    if (!productoExiste)
        return Results.NotFound(new { error = "Producto no encontrado." });

    // Si es portada quitar el flag a las demás
    if (dto.EsPortada)
    {
        var anteriores = await db.Imagenes
            .Where(i => i.IdProducto == id && i.EsPortada)
            .ToListAsync();
        anteriores.ForEach(i => i.EsPortada = false);
    }

    var imagen = new CAT_ProductoImagen
    {
        IdProducto = id,
        UrlImagen = dto.UrlImagen,
        EsPortada = dto.EsPortada,
        Status = 1
    };

    db.Imagenes.Add(imagen);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/catalogo/productos/{id}/imagenes/{imagen.Id}",
        new { imagen.Id, imagen.UrlImagen, imagen.EsPortada });
})
.WithName("AgregarImagen")
.WithTags("Productos")
.WithSummary("Agrega una imagen a un producto");

// GET /api/catalogo/descuento
app.MapGet("/api/catalogo/descuento", (decimal monto, bool esElectronica) =>
{
    if (monto <= 0)
        return Results.BadRequest(new
        { error = "El monto debe ser mayor a cero." });

    var resultado = CalculadorDescuento.CalcularConDetalle(monto, esElectronica);
    return Results.Ok(resultado);
})
.WithName("CalcularDescuento")
.WithTags("Descuentos")
.WithSummary("Calcula el descuento aplicable")
.WithDescription("10% general o 5% en Electrónica para compras >= $10,000 MXN");

app.Run();