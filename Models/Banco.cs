using System;
using System.Collections.Generic;

namespace FinTrack.Models;

public partial class Banco
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public virtual ICollection<Divida> Dividas { get; set; } = new List<Divida>();
}
