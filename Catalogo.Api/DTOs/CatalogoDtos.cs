namespace Catalogo.Api.DTOs;

public record CrearCategoriaDto(
    string Nombre,
    bool EsElectronica
);

public record CrearMarcaDto(
    string Nombre
);

public record CrearProductoDto(
    string Nombre,
    string? Descripcion,
    decimal PrecioVenta,
    int Stock,
    int IdCategoria,
    int IdMarca
);

public record AgregarImagenDto(
    string UrlImagen,
    bool EsPortada
);