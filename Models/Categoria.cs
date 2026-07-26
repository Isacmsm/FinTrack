using System;
using System.Collections.Generic;

namespace FinTrack.Models;

public partial class Categoria
{
    public int Id { get; set; }

    public bool Tipo { get; set; }

    public string Nome { get; set; } = null!;

    public int? IdUser { get; set; }

    public virtual Usuario? IdUserNavigation { get; set; }

    public virtual ICollection<Recorrente> Recorrentes { get; set; } = new List<Recorrente>();

    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
