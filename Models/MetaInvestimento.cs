using System;

namespace FinTrack.Models;

/// <summary>
/// Meta de investimento (ex.: "Reserva de emergência"). Progresso não é
/// lançado à mão: é a soma do saldo de <see cref="PluggyInvestimento"/> do
/// usuário (ACTIVE/PENDING, igual à página Aplicações), opcionalmente restrita
/// a um <see cref="TipoScope"/>. Não linka posições específicas porque
/// conectores como o Meu Pluggy fecham e reabrem uma posição de CDB por dia —
/// um vínculo por InvestmentId apodreceria em menos de 24h.
/// </summary>
public partial class MetaInvestimento
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string Nome { get; set; } = null!;

    public decimal ValorAlvo { get; set; }

    public DateTime? DataAlvo { get; set; }

    /// <summary>Tipo da Pluggy (FIXED_INCOME, EQUITY, ...) pra restringir o saldo somado. Null = todos.</summary>
    public string? TipoScope { get; set; }

    public DateTime CriadoEm { get; set; }

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
