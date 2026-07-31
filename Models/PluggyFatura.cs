using System;

namespace FinTrack.Models;

/// <summary>
/// Fatura real de cartão de crédito, vinda de GET /bills da Pluggy — não
/// inferida a partir das transações importadas. O valor total é o mesmo que
/// o meu.pluggy.ai mostra, mesmo pra períodos anteriores aos 90 dias de
/// transações sincronizadas (aí a fatura existe com total certo, mas sem
/// lista de compras porque essas transações nunca foram importadas).
/// </summary>
public partial class PluggyFatura
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string BillId { get; set; } = null!;

    public string ContaPluggyId { get; set; } = null!;

    public string? ContaNome { get; set; }

    public DateTime DataVencimento { get; set; }

    public DateTime? DataFechamento { get; set; }

    public decimal ValorTotal { get; set; }

    public decimal? ValorMinimo { get; set; }

    /// <summary>
    /// Soma de bill.payments — pode vir sempre zero mesmo pra fatura paga de
    /// verdade, porque o campo não aparece em nenhum exemplo da documentação
    /// da Pluggy pra esse tipo de conector. Por isso "Paga" não vira uma
    /// afirmação — ver <see cref="StatusPagamento"/>.
    /// </summary>
    public decimal ValorPago { get; set; }

    public DateTime? DataUltimoPagamento { get; set; }

    public string StatusPagamento => ValorPago >= ValorTotal - 0.02m && ValorTotal > 0
        ? "Paga"
        : ValorPago > 0
            ? "Parcialmente paga"
            : "Sem confirmação de pagamento";

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
