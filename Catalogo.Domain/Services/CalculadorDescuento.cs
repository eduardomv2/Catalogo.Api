using System;
using System.Collections.Generic;
using System.Text;

namespace Catalogo.Domain.Services;

public static class CalculadorDescuento
{
    private const decimal UmbralDescuento = 10_000m;
    private const decimal TasaGeneral = 0.10m;
    private const decimal TasaElectronica = 0.05m;

    public static decimal Calcular(decimal montoCompra, bool esElectronica)
    {
        if (montoCompra < UmbralDescuento) return 0m;
        return esElectronica ? TasaElectronica : TasaGeneral;
    }

    public static ResultadoDescuento CalcularConDetalle(
        decimal montoCompra, bool esElectronica)
    {
        var tasa = Calcular(montoCompra, esElectronica);
        var montoDescontado = Math.Round(montoCompra * tasa, 2);
        var totalFinal = montoCompra - montoDescontado;
        return new ResultadoDescuento(tasa, montoDescontado, totalFinal);
    }
}

public record ResultadoDescuento(
    decimal Tasa,
    decimal MontoDescontado,
    decimal TotalFinal);
