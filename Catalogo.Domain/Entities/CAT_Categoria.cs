using System;
using System.Collections.Generic;
using System.Text;

namespace Catalogo.Domain.Entities;

public class CAT_Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsElectronica { get; set; } = false;
    public int Status { get; set; } = 1;
}