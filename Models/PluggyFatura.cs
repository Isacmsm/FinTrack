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
    /// Soma crua de <c>bill.payments[]</c>, guardada só como registro do que a
    /// API devolveu. <b>Não é "o quanto esta fatura foi paga"</b>: a Pluggy põe
    /// em payments todo pagamento que caiu na janela da fatura, inclusive o que
    /// quitou a anterior e o que adiantou a seguinte — daí sair maior que
    /// <see cref="ValorTotal"/> em 4 das 6 faturas reais que testei.
    ///
    /// Quem responde "esta fatura foi paga?" é a
    /// <see cref="FinTrack.Data.FaturaCalculadora"/>, a partir de
    /// <see cref="PluggyFaturaPagamento"/>.
    /// </summary>
    public decimal ValorPago { get; set; }

    /// <inheritdoc cref="ValorPago"/>
    public DateTime? DataUltimoPagamento { get; set; }

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
