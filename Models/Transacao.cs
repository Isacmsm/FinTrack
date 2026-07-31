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

    public string? PluggyTransactionId { get; set; }

    /// <summary>
    /// Categoria e categoryId originais da Pluggy, só pra observação — ainda
    /// não usados em nenhuma regra. Guardados pra dar pra ver, com dados
    /// reais, se a taxonomia da Pluggy tem valores tipo "Investments" que
    /// dariam uma regra de detecção melhor que casar texto de descrição.
    /// </summary>
    public string? PluggyCategoria { get; set; }

    public string? PluggyCategoriaId { get; set; }

    /// <summary>
    /// Comércio extraído pela Pluggy (transaction.merchant) — separado da
    /// descrição bruta, então não sofre com variação de sufixo tipo "IFOOD
    /// *IFOOD 4471" vs "IFOOD *IFOOD 9902". Nem toda transação tem merchant
    /// (o exemplo da própria documentação da Pluggy já vem com null).
    /// </summary>
    public string? MerchantNome { get; set; }

    public string? MerchantCnpj { get; set; }

    /// <summary>
    /// Conta/cartão de origem na Pluggy (account.id/name/type) — antes só o
    /// nome da conta era usado, como fallback de descrição, e se perdia.
    /// Guardado pra viabilizar a seção Cartões (agrupar por conta).
    /// </summary>
    public string? ContaPluggyId { get; set; }

    public string? ContaNome { get; set; }

    public string? ContaTipo { get; set; }

    /// <summary>
    /// creditCardMetadata da Pluggy — billId/billForecastDate só vêm em
    /// conectores Open Finance segundo a documentação; pra Meu Pluggy podem
    /// ficar nulos, e aí Faturas cai pro fallback de agrupar por
    /// (conta, mês/ano de Data). Parcelamento (installmentNumber/Total)
    /// já foi confirmado com o próprio Item Meu Pluggy do usuário.
    /// </summary>
    public string? PluggyBillId { get; set; }

    /// <summary>Formato YYYY-MM devolvido pela Pluggy, não uma data completa.</summary>
    public string? PluggyFaturaPrevista { get; set; }

    public int? PluggyParcelaAtual { get; set; }

    public int? PluggyParcelaTotal { get; set; }

    public decimal? PluggyValorCompraTotal { get; set; }

    public DateTime? PluggyDataCompra { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
