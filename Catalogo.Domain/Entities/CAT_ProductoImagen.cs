using System;
using System.Collections.Generic;
using System.Text;

namespace Catalogo.Domain.Entities;

public class CAT_ProductoImagen
{
    public int Id { get; set; }
    public int IdProducto { get; set; }
    public string UrlImagen { get; set; } = string.Empty;
    public bool EsPortada { get; set; } = false;
    public int Status { get; set; } = 1;

    public CAT_Producto Producto { get; set; } = null!;
}