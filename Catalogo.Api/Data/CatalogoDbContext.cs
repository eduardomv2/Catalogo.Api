using Catalogo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Data;

public class CatalogoDbContext : DbContext
{
    public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options)
        : base(options) { }

    public DbSet<CAT_Categoria> Categorias => Set<CAT_Categoria>();
    public DbSet<CAT_Marca> Marcas => Set<CAT_Marca>();
    public DbSet<CAT_Producto> Productos => Set<CAT_Producto>();
    public DbSet<CAT_ProductoImagen> Imagenes => Set<CAT_ProductoImagen>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<CAT_Categoria>(e =>
        {
            e.ToTable("CAT_Categoria");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        });

        m.Entity<CAT_Marca>(e =>
        {
            e.ToTable("CAT_Marca");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        });

        m.Entity<CAT_Producto>(e =>
        {
            e.ToTable("CAT_Producto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            e.Property(x => x.PrecioVenta).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Categoria)
             .WithMany()
             .HasForeignKey(x => x.IdCategoria)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Marca)
             .WithMany()
             .HasForeignKey(x => x.IdMarca)
             .OnDelete(DeleteBehavior.Restrict);
        });

        m.Entity<CAT_ProductoImagen>(e =>
        {
            e.ToTable("CAT_ProductoImagen");
            e.HasKey(x => x.Id);
            e.Property(x => x.UrlImagen).IsRequired().HasMaxLength(500);
            e.HasOne(x => x.Producto)
             .WithMany(p => p.Imagenes)
             .HasForeignKey(x => x.IdProducto)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}