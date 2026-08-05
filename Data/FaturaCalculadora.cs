using FinTrack.Models;

namespace FinTrack.Data;

/// <summary>
/// Resumo de uma fatura de cartão pra exibição — usado por Cartões e por
/// Faturas, que precisam mostrar exatamente os mesmos números (compras,
/// pagamento, adiantamento). Ver <see cref="FaturaCalculadora.Calcular"/>.
/// </summary>
public class FaturaResumo
{
    public string ContaPluggyId { get; set; } = "";
    public string ContaNome { get; set; } = "";
    public DateTime? DataVencimento { get; set; }
    public DateTime? DataFechamento { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal? ValorMinimo { get; set; }

    /// <summary>Só compras/estornos de verdade — pagamento de fatura (movimentação interna) nunca entra aqui.</summary>
    public List<Transacao> Transacoes { get; set; } = [];

    /// <summary>true = calculada localmente (compras desde o último fechamento conhecido), não é uma Bill real da Pluggy ainda.</summary>
    public bool Sintetizada { get; set; }

    public string StatusPagamento { get; set; } = "";
    public bool PagamentoVistoNoExtrato { get; set; }

    /// <summary>Só preenchido pro ciclo em aberto — Nubank (e outros) deixam adiantar parte da fatura antes dela fechar.</summary>
    public List<Transacao> PagamentosAntecipados { get; set; } = [];
    public decimal ValorAntecipado => PagamentosAntecipados.Sum(p => p.Valor);
    public decimal SaldoRestante => ValorTotal - ValorAntecipado;

    /// <summary>Despesa soma, estorno/receita subtrai — igual o efeito real na fatura.</summary>
    public decimal SomaTransacoesImportadas => Transacoes.Sum(t => t.IdCategoriaNavigation.Tipo ? t.Valor : -t.Valor);
    public bool DivergeDoTotal => !Sintetizada && Math.Abs(SomaTransacoesImportadas - ValorTotal) > 0.02m;
}

/// <summary>
/// Monta a lista de faturas (fechadas, vindas de PluggyFatura/GET bills, +
/// uma sintetizada pro ciclo em aberto) pra um usuário. Cartões e Faturas
/// chamam isso — nenhuma das duas páginas recalcula por conta própria, pra
/// não voltar a divergir uma da outra como já aconteceu.
/// </summary>
public static class FaturaCalculadora
{
    public const int IdCategoriaRecebimentoFatura = 19;

    public static List<FaturaResumo> Calcular(List<PluggyFatura> faturasPluggy, List<Transacao> transacoesCartao, DateTime hoje)
    {
        // "Pagamento recebido" é a Pluggy registrando, no próprio extrato do
        // cartão, que uma fatura anterior foi paga — não é uma compra, e não
        // pertence ao ciclo em que a data cai (o pagamento de uma fatura
        // acontece bem depois dela fechar). Fica de fora da lista de compras
        // e vira evidência de pagamento (bills.payments pode vir vazio pra
        // algumas faturas — isso aqui é o fallback comparando valor).
        var pagamentosRecebidos = transacoesCartao.Where(t => t.IdCategoria == IdCategoriaRecebimentoFatura).ToList();

        var resultado = new List<FaturaResumo>();

        var contasComFaturaOuTransacao = faturasPluggy.Select(f => f.ContaPluggyId)
            .Union(transacoesCartao.Select(t => t.ContaPluggyId!))
            .Distinct();

        foreach (var idConta in contasComFaturaOuTransacao)
        {
            var nomeConta = faturasPluggy.FirstOrDefault(f => f.ContaPluggyId == idConta)?.ContaNome
                             ?? transacoesCartao.FirstOrDefault(t => t.ContaPluggyId == idConta)?.ContaNome
                             ?? "Cartão";

            // Cada transação de cartão já vem da Pluggy com o BillId da fatura
            // a que pertence (CreditCardMetadata.billId) — é o dado oficial
            // dela, não uma aproximação nossa. Só cai na janela de data quem
            // ainda não tem BillId (o ciclo em aberto, que a Pluggy ainda não
            // fechou como Bill). Uma janela de data por conta própria já
            // errou compra parcelada (forecast pra fatura futura, sem BillId
            // ainda) pra dentro da fatura fechada errada.
            var ordenadas = faturasPluggy.Where(f => f.ContaPluggyId == idConta)
                .OrderBy(f => f.DataFechamento ?? f.DataVencimento)
                .ToList();
            DateTime? fechamentoAnterior = null;
            var idsPagamentosUsados = new HashSet<int>();

            foreach (var fatura in ordenadas)
            {
                var transacoesDaFatura = transacoesCartao
                    .Where(t => t.ContaPluggyId == idConta && !t.IdCategoriaNavigation.EhMovimentacaoInterna
                                && t.PluggyBillId == fatura.BillId)
                    .OrderByDescending(t => t.Data)
                    .ToList();

                // bills.payments pode vir vazio pra essa fatura ainda. Fallback:
                // procura no próprio extrato um "Recebimento de Fatura" com
                // valor batendo (tolerância de juro/arredondamento) depois do
                // fechamento.
                var pagamentoVisto = fatura.StatusPagamento != "Paga" && fatura.DataFechamento is not null
                    ? pagamentosRecebidos
                        .Where(p => p.ContaPluggyId == idConta && !idsPagamentosUsados.Contains(p.Id) && p.Data >= fatura.DataFechamento)
                        .OrderBy(p => Math.Abs(p.Valor - fatura.ValorTotal))
                        .FirstOrDefault(p => Math.Abs(p.Valor - fatura.ValorTotal) <= 2m)
                    : null;

                if (pagamentoVisto is not null) idsPagamentosUsados.Add(pagamentoVisto.Id);

                resultado.Add(new FaturaResumo
                {
                    ContaPluggyId = idConta,
                    ContaNome = nomeConta,
                    DataVencimento = fatura.DataVencimento,
                    DataFechamento = fatura.DataFechamento,
                    ValorTotal = fatura.ValorTotal,
                    ValorMinimo = fatura.ValorMinimo,
                    Transacoes = transacoesDaFatura,
                    StatusPagamento = pagamentoVisto is not null ? "Paga" : fatura.StatusPagamento,
                    PagamentoVistoNoExtrato = pagamentoVisto is not null
                });

                fechamentoAnterior = fatura.DataFechamento ?? fechamentoAnterior;
            }

            // A Pluggy ainda não fechou uma Bill pro ciclo corrente, então
            // essas transações chegam sem BillId — calculado aqui a partir
            // delas (só compra/estorno, pagamento de fatura continua de
            // fora), e marcado como estimativa, não dado oficial. A data
            // continua como cinto de segurança (não deixa uma transação
            // muito antiga sem BillId, por qualquer falha da Pluggy, inflar
            // o ciclo aberto).
            var transacoesEmAberto = transacoesCartao
                .Where(t => t.ContaPluggyId == idConta && !t.IdCategoriaNavigation.EhMovimentacaoInterna
                            && t.PluggyBillId == null
                            && t.Data > (fechamentoAnterior ?? DateTime.MinValue) && t.Data <= hoje)
                .OrderByDescending(t => t.Data)
                .ToList();

            // Nubank (e outros) deixam adiantar parte da fatura antes dela
            // fechar. Um "Recebimento de Fatura" que já não foi usado pra
            // confirmar pagamento de uma fatura fechada, e que caiu dentro
            // dessa janela em aberto, é um adiantamento — não uma compra.
            var pagamentosAntecipados = pagamentosRecebidos
                .Where(p => p.ContaPluggyId == idConta && !idsPagamentosUsados.Contains(p.Id)
                            && p.Data > (fechamentoAnterior ?? DateTime.MinValue) && p.Data <= hoje)
                .OrderByDescending(p => p.Data)
                .ToList();

            if (transacoesEmAberto.Count > 0 || pagamentosAntecipados.Count > 0)
            {
                resultado.Add(new FaturaResumo
                {
                    ContaPluggyId = idConta,
                    ContaNome = nomeConta,
                    DataVencimento = null,
                    DataFechamento = null,
                    ValorTotal = transacoesEmAberto.Sum(t => t.IdCategoriaNavigation.Tipo ? t.Valor : -t.Valor),
                    ValorMinimo = null,
                    Transacoes = transacoesEmAberto,
                    Sintetizada = true,
                    StatusPagamento = "",
                    PagamentosAntecipados = pagamentosAntecipados
                });
            }
        }

        return resultado.OrderByDescending(f => f.DataVencimento ?? DateTime.MaxValue).ToList();
    }

    public static (string Texto, string Classe) BadgeStatusPagamento(string status) => status switch
    {
        "Paga" => ("Paga", "text-bg-success"),
        "Parcialmente paga" => ("Parcialmente paga", "text-bg-warning"),
        _ => ("Sem confirmação de pagamento", "text-bg-light border text-secondary")
    };
}
