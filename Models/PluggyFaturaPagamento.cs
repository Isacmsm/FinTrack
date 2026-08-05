using System;

namespace FinTrack.Models;

/// <summary>
/// Um pagamento de fatura de cartão, como evento isolado — não "o quanto a
/// fatura X recebeu".
///
/// Essa distinção é o ponto todo: <c>bills[].payments[]</c> da Pluggy lista,
/// dentro de cada fatura, todo pagamento que caiu na janela dela, inclusive os
/// que quitaram a fatura anterior e os que adiantaram a seguinte. Confirmado
/// no extrato real: a fatura de R$ 1.966,14 vem com R$ 2.736,14 em
/// <c>payments</c>, um deles feito depois da fatura seguinte já ter fechado.
/// Somar isso e chamar de "valor pago" dá número maior que o total da fatura.
///
/// Então o vínculo pagamento → fatura não é guardado: é recalculado em
/// <see cref="FinTrack.Data.FaturaCalculadora"/> a partir das datas e dos
/// saldos, do jeito que um cartão funciona de verdade.
///
/// <para>
/// A chave natural (IdUser + conta + data + valor) existe porque a Pluggy não
/// dá um id estável pra pagamento: o MESMO pagamento aparece com ids
/// diferentes em faturas diferentes, e o mesmo pagamento aparece de novo como
/// transação no extrato do cartão (às vezes duplicado ali também). Dois
/// pagamentos idênticos no mesmo dia, na mesma conta e no mesmo centavo viram
/// um só — é o preço de deduplicar sem id confiável, e o erro que isso causa é
/// bem menor que contar o mesmo pagamento duas ou três vezes.
/// </para>
/// </summary>
public partial class PluggyFaturaPagamento
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string ContaPluggyId { get; set; } = null!;

    /// <summary>Só a data (sem hora) — é o que a Pluggy devolve em bills[].payments[].</summary>
    public DateTime Data { get; set; }

    /// <summary>Sempre positivo (quanto foi pago), independente do sinal na origem.</summary>
    public decimal Valor { get; set; }

    /// <summary>"bill" (veio de bills[].payments[]) ou "extrato" (veio de uma transação do cartão). Só pra diagnóstico.</summary>
    public string Origem { get; set; } = null!;

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
