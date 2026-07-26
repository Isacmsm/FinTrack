using System;
using System.Collections.Generic;

namespace FinTrack.Models;

public partial class Recorrente
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string Descricao { get; set; } = null!;

    public int IdCategoria { get; set; }

    public decimal Valor { get; set; }

    public int TipoVencimento { get; set; }

    public int DiaVencimento { get; set; }

    public bool Ativo { get; set; }

    public virtual ICollection<Divida> Dividas { get; set; } = new List<Divida>();

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
