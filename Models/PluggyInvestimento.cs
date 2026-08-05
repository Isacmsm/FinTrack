using System;

namespace FinTrack.Models;

/// <summary>
/// Posição de investimento vinda de GET /investments da Pluggy — CDB, Tesouro
/// Direto, fundos, ações etc. Igual a <see cref="PluggyFatura"/>, é fonte
/// própria (não derivada das transações importadas).
///
/// Upsert por <see cref="InvestmentId"/>: uma posição que sai da resposta da
/// Pluggy não é excluída daqui, só some de uma futura sincronização — por
/// isso <see cref="Status"/> é persistido e "TOTAL_WITHDRAWAL" não deve
/// entrar em nenhum total exibido. Bancos como o Nubank fecham e reabrem uma
/// posição de CDB por dia pro "cofrinho" automático, então a maioria das
/// linhas com esse status é saldo zerado histórico, não dinheiro perdido.
/// </summary>
public partial class PluggyInvestimento
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string InvestmentId { get; set; } = null!;

    public string? NomeConector { get; set; }

    public string Tipo { get; set; } = null!;

    public string? Subtipo { get; set; }

    public string Nome { get; set; } = null!;

    public string? Codigo { get; set; }

    public string? Isin { get; set; }

    public decimal Saldo { get; set; }

    public decimal? ValorOriginal { get; set; }

    /// <summary>
    /// Costuma vir nulo mesmo pra posição ativa (ex.: CDB) — quando nulo, a
    /// página calcula Saldo - ValorOriginal em vez de mostrar "—" direto.
    /// </summary>
    public decimal? LucroInformado { get; set; }

    public decimal? ValorDisponivelResgate { get; set; }

    public decimal? Quantidade { get; set; }

    public decimal? ValorCota { get; set; }

    public string? MoedaCodigo { get; set; }

    public DateTime Data { get; set; }

    public DateTime? DataVencimento { get; set; }

    public DateTime? DataAplicacao { get; set; }

    public decimal? Taxa { get; set; }

    public string? TipoTaxa { get; set; }

    public decimal? TaxaAnualFixa { get; set; }

    public string? Emissor { get; set; }

    public string Status { get; set; } = null!;

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
