using System;
using System.Collections.Generic;
using System.Text;

namespace Catalogo.Domain.Entities;

public class CAT_Producto
{
    public int Id { get; set; }
    public int IdCategoria { get; set; }
    public int IdMarca { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioVenta { get; set; } = 0m;
    public int Stock { get; set; } = 0;
    public int Status { get; set; } = 1;

    public CAT_Categoria Categoria { get; set; } = null!;
    public CAT_Marca Marca { get; set; } = null!;
    public ICollection<CAT_ProductoImagen> Imagenes { get; set; } = [];
}
