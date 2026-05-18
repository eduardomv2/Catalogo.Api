using System;
using System.Collections.Generic;
using System.Text;
using Catalogo.Domain.Entities;

namespace Catalogo.Domain.Services;

public static class ValidadorProducto
{
    public static bool Validar(CAT_Producto producto)
        => ObtenerErrores(producto).Count == 0;

    public static IReadOnlyList<string> ObtenerErrores(CAT_Producto producto)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(producto.Nombre))
            errores.Add("El nombre del producto no puede estar vacío.");

        if (producto.PrecioVenta <= 0)
            errores.Add("El precio de venta debe ser mayor a cero.");

        if (producto.Stock < 0)
            errores.Add("El stock no puede ser negativo.");

        return errores;
    }
}