using System;

namespace FinTrack.Models;

// Histórico de sessões pro painel /admin ("quando cada um logou / ficou
// online"). FimEm é sempre uma estimativa — ver os três valores de Motivo:
// "logout" (preciso, veio de Pages/ControleAcesso/Sair.cshtml), "heartbeat"
// (a sessão parou de mandar heartbeat — fechada com o último ping conhecido,
// Pages/Admin/Index.cshtml) ou "restart" (o processo caiu/reiniciou com a
// sessão ainda aberta — Program.cs fecha tudo que sobrou no boot, porque o
// PresenceTracker em memória, que o fechamento por heartbeat depende, não
// sobrevive a restart).
public partial class SessaoUsuario
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string Token { get; set; } = null!;

    public DateTime InicioEm { get; set; }

    public DateTime? FimEm { get; set; }

    public string? Motivo { get; set; }

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
