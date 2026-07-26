using System;
using System.Collections.Generic;

namespace FinTrack.Models;

public partial class Transacao
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string? Desc { get; set; }

    public int IdCategoria { get; set; }

    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
