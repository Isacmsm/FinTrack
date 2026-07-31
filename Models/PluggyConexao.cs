using System;

namespace FinTrack.Models;

public partial class PluggyConexao
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string ClientIdProtegido { get; set; } = null!;

    public string ClientSecretProtegido { get; set; } = null!;

    public DateTime CriadoEm { get; set; }

    public DateTime AtualizadoEm { get; set; }

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
