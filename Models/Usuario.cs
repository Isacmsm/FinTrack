using System;
using System.Collections.Generic;

namespace FinTrack.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public virtual ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();

    public virtual ICollection<Divida> Dividas { get; set; } = new List<Divida>();

    public virtual ICollection<Recorrente> Recorrentes { get; set; } = new List<Recorrente>();

    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
