using FinTrack.Models;

namespace FinTrack.Data;

public enum EstadoFatura
{
    /// <summary>Ciclo já fechou — o que está aqui não muda mais.</summary>
    Fechada,

    /// <summary>Ciclo que contém hoje: ainda entra compra nele.</summary>
    Aberta,

    /// <summary>Ciclo que ainda nem começou — só tem parcela futura de compra já feita.</summary>
    Futura
}

/// <summary>Um pagamento já atribuído a um ciclo pelo <see cref="FaturaCalculadora"/>.</summary>
public record PagamentoAplicado(DateTime Data, decimal Valor);

/// <summary>
/// Resumo de um ciclo de fatura pra exibição — usado por Cartões e por
/// Faturas, que precisam mostrar exatamente os mesmos números.
/// </summary>
public class FaturaResumo
{
    public string ContaPluggyId { get; set; } = "";
    public string ContaNome { get; set; } = "";

    public DateTime DataFechamento { get; set; }
    public DateTime DataVencimento { get; set; }

    public EstadoFatura Estado { get; set; }

    /// <summary>
    /// true = existe uma Bill de verdade da Pluggy pra esse ciclo, e
    /// <see cref="ValorTotal"/> é o valor oficial dela. false = ciclo
    /// projetado pelo ritmo do cartão (o aberto, os futuros, e buracos que a
    /// Pluggy não devolveu), com total somado das transações.
    /// </summary>
    public bool TemFaturaOficial { get; set; }

    public decimal? ValorMinimo { get; set; }

    /// <summary>Só compras/estornos — pagamento de fatura (movimentação interna) nunca entra aqui.</summary>
    public List<Transacao> Transacoes { get; set; } = [];

    /// <summary>Pagamentos feitos DEPOIS do fechamento: são a quitação desta fatura.</summary>
    public List<PagamentoAplicado> Pagamentos { get; set; } = [];

    /// <summary>
    /// Pagamentos feitos DENTRO do ciclo, antes de fechar. O banco já desconta
    /// isso do total da fatura — conferido no extrato: fatura de compras
    /// R$ 2.883,24 com R$ 700 adiantados fecha em R$ 2.183,27. Por isso eles
    /// entram no cálculo do total, não em cima dele.
    /// </summary>
    public List<PagamentoAplicado> Adiantamentos { get; set; } = [];

    /// <summary>
    /// Existe algum pagamento conhecido nessa conta? Se não, a fonte de
    /// pagamento não funciona pro conector e "não achei pagamento" não é
    /// evidência de fatura em aberto — ver <see cref="StatusPagamento"/>.
    /// </summary>
    public bool PagamentosConhecidosNaConta { get; set; }

    /// <summary>Total oficial da Bill. Null nos ciclos projetados.</summary>
    public decimal? TotalOficial { get; set; }

    /// <summary>Despesa soma, estorno/receita subtrai — igual o efeito real na fatura.</summary>
    public decimal ValorCompras => Transacoes.Sum(t => t.IdCategoriaNavigation.Tipo ? t.Valor : -t.Valor);

    public decimal ValorAntecipado => Adiantamentos.Sum(p => p.Valor);

    /// <summary>O que a fatura cobra — já líquido dos adiantamentos, como o banco faz.</summary>
    public decimal ValorTotal => TotalOficial ?? ValorCompras - ValorAntecipado;

    public decimal ValorPago => Pagamentos.Sum(p => p.Valor);

    public decimal SaldoRestante => ValorTotal - ValorPago;

    public DateTime? DataUltimoPagamento => Pagamentos.Count == 0 ? null : Pagamentos.Max(p => p.Data);

    /// <summary>
    /// Compara o que foi importado com o total oficial — só faz sentido onde
    /// existe total oficial pra comparar. A conta inclui os adiantamentos
    /// porque eles já estão embutidos no total do banco.
    /// </summary>
    /// <remarks>
    /// Zero transação importada não é divergência: é fatura anterior à janela
    /// de 90 dias da importação, e a própria tela já diz isso.
    /// </remarks>
    public bool DivergeDoTotal =>
        Transacoes.Count > 0
        && TotalOficial is decimal oficial
        && Math.Abs(ValorCompras - ValorAntecipado - oficial) > 0.05m;

    public string StatusPagamento
    {
        get
        {
            if (Estado != EstadoFatura.Fechada) return "";
            if (ValorTotal <= 0.02m) return "Paga";
            if (ValorPago >= ValorTotal - 0.02m) return "Paga";
            if (ValorPago > 0) return "Parcialmente paga";

            // Zero pagamento tem duas leituras opostas, e a diferença é se a
            // fonte de pagamento funciona pra essa conta: se outras faturas do
            // mesmo cartão têm pagamento, "nenhum aqui" é uma afirmação (está
            // em aberto). Se nenhuma tem, é só a Pluggy não devolvendo esse
            // dado pro conector, e afirmar atraso seria mentira.
            if (!PagamentosConhecidosNaConta) return "Sem confirmação de pagamento";

            return "Em aberto";
        }
    }

    /// <summary>Fatura fechada, não paga e já passou do vencimento.</summary>
    public bool EmAtraso { get; set; }
}

/// <summary>
/// Monta a linha do tempo de faturas de um cartão a partir de três fontes, sem
/// nenhuma regra específica de banco:
///
/// <list type="number">
/// <item><b>Ciclos</b> — cada Bill da Pluggy dá um par (fechamento, vencimento).
/// O ritmo entre elas é mensal, então dá pra projetar o ciclo aberto, os
/// futuros (onde caem as parcelas) e tapar buracos que a Pluggy não devolveu.</item>
/// <item><b>Transações</b> — cada uma cai em exatamente um ciclo: pelo BillId
/// quando a Pluggy manda (dado oficial), senão pelo mês de vencimento previsto
/// (BillForecastDate), senão pela data dentro da janela do ciclo. Conector que
/// não mande metadata nenhuma ainda funciona pelo terceiro critério.</item>
/// <item><b>Pagamentos</b> — aplicados como um cartão aplica de verdade: cada
/// pagamento abate a fatura fechada mais recente que ainda tem saldo, e o que
/// sobrar vira adiantamento do ciclo em curso. Nada de casar por valor.</item>
/// </list>
///
/// <para>
/// O que essa regra substitui: a Pluggy marca o pagamento com o BillId do
/// ciclo em que ele caiu, não o da fatura que ele quitou, e
/// <c>bills[].payments[]</c> mistura os pagamentos das faturas vizinha. As
/// duas coisas levaram a casar pagamento por valor com tolerância — que
/// quebrava a cada caso novo (pagamento dividido em dois, adiantamento,
/// pagamento em atraso). Aqui não existe tolerância de valor nenhuma.
/// </para>
/// </summary>
public static class FaturaCalculadora
{
    public const int IdCategoriaRecebimentoFatura = 19;

    /// <summary>Fallback pro intervalo fechamento→vencimento quando a Bill não traz o fechamento.</summary>
    private const int DiasFechamentoVencimentoPadrao = 7;

    /// <summary>Acima disso, dois fechamentos consecutivos não são ciclos vizinhos — tem fatura faltando no meio.</summary>
    private const int DiasMaximoEntreCiclos = 45;

    private class Ciclo
    {
        public DateTime Fechamento;
        public DateTime Vencimento;

        /// <summary>Início exclusivo da janela: o fechamento do ciclo anterior.</summary>
        public DateTime InicioExclusivo;

        public PluggyFatura? Bill;
        public FaturaResumo Resumo = new();
    }

    public static List<FaturaResumo> Calcular(
        List<PluggyFatura> faturasPluggy,
        List<Transacao> transacoesCartao,
        List<PluggyFaturaPagamento> pagamentos,
        DateTime hoje)
    {
        var resultado = new List<FaturaResumo>();

        var contas = faturasPluggy.Select(f => f.ContaPluggyId)
            .Union(transacoesCartao.Where(t => t.ContaPluggyId is not null).Select(t => t.ContaPluggyId!))
            .Union(pagamentos.Select(p => p.ContaPluggyId))
            .Distinct();

        foreach (var idConta in contas)
        {
            var billsDaConta = faturasPluggy.Where(f => f.ContaPluggyId == idConta).ToList();

            // Pagamento de fatura é movimentação interna: nunca entra na lista
            // de compras. A quitação vem da lista de pagamentos, não daqui.
            var compras = transacoesCartao
                .Where(t => t.ContaPluggyId == idConta && !t.IdCategoriaNavigation.EhMovimentacaoInterna)
                .ToList();

            var pagamentosDaConta = pagamentos
                .Where(p => p.ContaPluggyId == idConta)
                .OrderBy(p => p.Data).ThenBy(p => p.Valor)
                .ToList();

            var nomeConta = billsDaConta.FirstOrDefault()?.ContaNome
                            ?? compras.FirstOrDefault()?.ContaNome
                            ?? "Cartão";

            var ciclos = MontarCiclos(billsDaConta, compras, pagamentosDaConta, hoje);
            if (ciclos.Count == 0) continue;

            foreach (var ciclo in ciclos)
            {
                ciclo.Resumo.ContaPluggyId = idConta;
                ciclo.Resumo.ContaNome = nomeConta;
                ciclo.Resumo.DataFechamento = ciclo.Fechamento;
                ciclo.Resumo.DataVencimento = ciclo.Vencimento;
                ciclo.Resumo.TemFaturaOficial = ciclo.Bill is not null;
                ciclo.Resumo.TotalOficial = ciclo.Bill?.ValorTotal;
                ciclo.Resumo.ValorMinimo = ciclo.Bill?.ValorMinimo;
                ciclo.Resumo.PagamentosConhecidosNaConta = pagamentosDaConta.Count > 0;
                ciclo.Resumo.Estado = ciclo.Fechamento < hoje.Date
                    ? EstadoFatura.Fechada
                    : ciclo.InicioExclusivo < hoje.Date
                        ? EstadoFatura.Aberta
                        : EstadoFatura.Futura;
            }

            DistribuirTransacoes(ciclos, compras);
            AplicarPagamentos(ciclos, pagamentosDaConta);

            foreach (var ciclo in ciclos)
            {
                var r = ciclo.Resumo;
                r.EmAtraso = r.Estado == EstadoFatura.Fechada
                             && r.PagamentosConhecidosNaConta
                             && r.SaldoRestante > 0.02m
                             && r.DataVencimento < hoje.Date;

                // Ciclo projetado e vazio não é fatura nenhuma — é só um mês
                // que passou sem a Pluggy devolver Bill e sem transação
                // importada. Mostrar isso como "fatura de R$ 0,00" seria ruído.
                if (r.TemFaturaOficial || r.Transacoes.Count > 0 || r.Pagamentos.Count > 0
                    || r.Adiantamentos.Count > 0 || r.Estado == EstadoFatura.Aberta)
                {
                    resultado.Add(r);
                }
            }
        }

        // Ciclo aberto primeiro (é o que a pessoa quer ver), depois as
        // fechadas da mais recente pra trás, e por último as futuras em ordem
        // cronológica — elas são projeção, não histórico.
        return resultado
            .OrderBy(f => f.Estado == EstadoFatura.Aberta ? 0 : f.Estado == EstadoFatura.Fechada ? 1 : 2)
            .ThenBy(f => f.Estado == EstadoFatura.Futura ? f.DataVencimento.Ticks : -f.DataVencimento.Ticks)
            .ToList();
    }

    /// <summary>
    /// A linha do tempo mensal do cartão. As Bills dão os pontos reais; o
    /// resto é projetado a partir do ritmo delas (mesmo dia de fechamento e de
    /// vencimento, mês a mês) pra frente, pra trás e nos buracos.
    /// </summary>
    private static List<Ciclo> MontarCiclos(
        List<PluggyFatura> bills,
        List<Transacao> compras,
        List<PluggyFaturaPagamento> pagamentos,
        DateTime hoje)
    {
        var ciclos = new List<Ciclo>();

        if (bills.Count > 0)
        {
            // Quando a Bill não traz fechamento, usa a distância média das que
            // trazem — é o ritmo do próprio cartão, não um chute fixo.
            var distancias = bills
                .Where(b => b.DataFechamento is not null)
                .Select(b => (b.DataVencimento - b.DataFechamento!.Value).Days)
                .Where(d => d is > 0 and < DiasMaximoEntreCiclos)
                .ToList();
            var distanciaPadrao = distancias.Count > 0
                ? (int)Math.Round(distancias.Average())
                : DiasFechamentoVencimentoPadrao;

            foreach (var bill in bills.OrderBy(b => b.DataVencimento))
            {
                ciclos.Add(new Ciclo
                {
                    Fechamento = (bill.DataFechamento ?? bill.DataVencimento.AddDays(-distanciaPadrao)).Date,
                    Vencimento = bill.DataVencimento.Date,
                    Bill = bill
                });
            }

            PreencherBuracos(ciclos);
        }
        else
        {
            // Nenhuma Bill: o conector não devolve /bills, ou é a primeira
            // sincronização. Deriva o ritmo do único dado que sobra — o mês de
            // vencimento previsto que vem em cada transação. Sem nem isso, não
            // dá pra afirmar ciclo nenhum e a página fica vazia (honesto).
            var meses = compras
                .Select(t => t.PluggyFaturaPrevista)
                .Where(m => m is { Length: 7 })
                .Distinct()
                .Select(m => DateTime.TryParse(m + "-01", out var d) ? d : (DateTime?)null)
                .Where(d => d is not null)
                .Select(d => d!.Value)
                .OrderBy(d => d)
                .ToList();

            if (meses.Count == 0) return [];

            foreach (var mes in meses)
            {
                ciclos.Add(new Ciclo
                {
                    Vencimento = mes,
                    Fechamento = mes.AddDays(-DiasFechamentoVencimentoPadrao)
                });
            }
        }

        EstenderParaTras(ciclos, compras, pagamentos);
        EstenderParaFrente(ciclos, compras, pagamentos, hoje);

        ciclos = ciclos.OrderBy(c => c.Fechamento).ToList();

        // Janela de cada ciclo: (fechamento anterior, fechamento]. O primeiro
        // não tem anterior, então vale um mês pra trás.
        for (var i = 0; i < ciclos.Count; i++)
        {
            ciclos[i].InicioExclusivo = i == 0
                ? ciclos[i].Fechamento.AddMonths(-1)
                : ciclos[i - 1].Fechamento;
        }

        return ciclos;
    }

    /// <summary>
    /// Meses sem Bill entre duas Bills. Sem isso, a janela da Bill seguinte
    /// engoliria meses inteiros de transação — a lista da Pluggy tem buraco
    /// (ex.: pula de dezembro pra abril).
    /// </summary>
    private static void PreencherBuracos(List<Ciclo> ciclos)
    {
        var comBill = ciclos.OrderBy(c => c.Fechamento).ToList();

        for (var i = 1; i < comBill.Count; i++)
        {
            var anterior = comBill[i - 1];
            var atual = comBill[i];
            var passo = 1;

            while ((atual.Fechamento - anterior.Fechamento.AddMonths(passo - 1)).Days > DiasMaximoEntreCiclos)
            {
                ciclos.Add(new Ciclo
                {
                    Fechamento = anterior.Fechamento.AddMonths(passo),
                    Vencimento = anterior.Vencimento.AddMonths(passo)
                });
                passo++;
            }
        }
    }

    private static void EstenderParaTras(List<Ciclo> ciclos, List<Transacao> compras, List<PluggyFaturaPagamento> pagamentos)
    {
        var primeiro = ciclos.OrderBy(c => c.Fechamento).First();

        var maisAntigo = DataMinima(compras, pagamentos);
        if (maisAntigo is null) return;

        var passo = 1;
        while (primeiro.Fechamento.AddMonths(-passo) >= maisAntigo.Value.Date && passo <= 24)
        {
            ciclos.Add(new Ciclo
            {
                Fechamento = primeiro.Fechamento.AddMonths(-passo),
                Vencimento = primeiro.Vencimento.AddMonths(-passo)
            });
            passo++;
        }
    }

    /// <summary>
    /// Ciclo aberto e ciclos futuros. Os futuros existem porque parcela de
    /// compra já feita vem com vencimento previsto meses à frente — sem eles,
    /// tudo isso caía no ciclo aberto e inflava o valor da fatura atual.
    /// </summary>
    private static void EstenderParaFrente(List<Ciclo> ciclos, List<Transacao> compras, List<PluggyFaturaPagamento> pagamentos, DateTime hoje)
    {
        var ultimo = ciclos.OrderBy(c => c.Fechamento).Last();

        var limite = new[]
            {
                hoje.Date,
                DataMaxima(compras, pagamentos) ?? hoje.Date,
                MesVencimentoMaximo(compras) ?? hoje.Date
            }
            .Max();

        var passo = 1;
        while (ultimo.Fechamento.AddMonths(passo - 1) < limite && passo <= 36)
        {
            ciclos.Add(new Ciclo
            {
                Fechamento = ultimo.Fechamento.AddMonths(passo),
                Vencimento = ultimo.Vencimento.AddMonths(passo)
            });
            passo++;
        }
    }

    private static DateTime? DataMinima(List<Transacao> compras, List<PluggyFaturaPagamento> pagamentos)
    {
        var datas = compras.Select(t => t.Data).Concat(pagamentos.Select(p => p.Data)).ToList();
        return datas.Count == 0 ? null : datas.Min();
    }

    private static DateTime? DataMaxima(List<Transacao> compras, List<PluggyFaturaPagamento> pagamentos)
    {
        var datas = compras.Select(t => t.Data).Concat(pagamentos.Select(p => p.Data)).ToList();
        return datas.Count == 0 ? null : datas.Max();
    }

    private static DateTime? MesVencimentoMaximo(List<Transacao> compras)
    {
        var meses = compras
            .Select(t => t.PluggyFaturaPrevista)
            .Where(m => m is { Length: 7 })
            .Select(m => DateTime.TryParse(m + "-01", out var d) ? d : (DateTime?)null)
            .Where(d => d is not null)
            .Select(d => d!.Value)
            .ToList();

        return meses.Count == 0 ? null : meses.Max();
    }

    /// <summary>
    /// Cada transação num ciclo só. BillId primeiro porque é a Pluggy dizendo
    /// em qual fatura ela entrou; o mês previsto depois (compra perto do
    /// fechamento e parcela futura só têm isso); a data por último, que é o
    /// único critério que funciona pra conector sem metadata de cartão.
    /// </summary>
    private static void DistribuirTransacoes(List<Ciclo> ciclos, List<Transacao> compras)
    {
        var porBillId = ciclos.Where(c => c.Bill is not null)
            .GroupBy(c => c.Bill!.BillId)
            .ToDictionary(g => g.Key, g => g.First());

        var porMesVencimento = ciclos
            .GroupBy(c => c.Vencimento.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var transacao in compras)
        {
            Ciclo? destino = null;

            if (transacao.PluggyBillId is { Length: > 0 } billId)
                porBillId.TryGetValue(billId, out destino);

            if (destino is null && transacao.PluggyFaturaPrevista is { Length: 7 } mes)
                porMesVencimento.TryGetValue(mes, out destino);

            destino ??= ciclos.FirstOrDefault(c => transacao.Data.Date > c.InicioExclusivo && transacao.Data.Date <= c.Fechamento)
                        ?? ciclos.Last();

            destino.Resumo.Transacoes.Add(transacao);
        }

        foreach (var ciclo in ciclos)
            ciclo.Resumo.Transacoes = ciclo.Resumo.Transacoes.OrderByDescending(t => t.Data).ToList();
    }

    /// <summary>
    /// Do jeito que um cartão funciona: o pagamento abate a fatura fechada
    /// mais recente que ainda deve. Se ela já está quitada (ou nem fechou), o
    /// dinheiro é adiantamento do ciclo em curso — e o banco desconta isso do
    /// total da próxima fatura, que é justamente por que o total oficial vem
    /// menor que a soma das compras.
    ///
    /// <para>
    /// Não cascateia pra faturas mais antigas de propósito: sobra de pagamento
    /// vira crédito no cartão, não quitação retroativa de uma fatura atrasada
    /// de meses atrás.
    /// </para>
    /// </summary>
    private static void AplicarPagamentos(List<Ciclo> ciclos, List<PluggyFaturaPagamento> pagamentos)
    {
        foreach (var pagamento in pagamentos)
        {
            var restante = pagamento.Valor;

            // A última fatura fechada que cobra alguma coisa. Ciclo fechado com
            // total zero é pulado (mês que a Pluggy não devolveu e do qual não
            // há transação importada) — senão uma fatura atrasada de verdade,
            // mais antiga, nunca receberia pagamento nenhum.
            var ultimaFechada = ciclos
                .Where(c => c.Fechamento <= pagamento.Data.Date && c.Resumo.ValorTotal > 0.02m)
                .OrderByDescending(c => c.Fechamento)
                .FirstOrDefault();

            // Só ela: se já está quitada, o dinheiro é adiantamento do ciclo em
            // curso, e NÃO quitação retroativa de uma fatura atrasada de meses
            // atrás. Foi exatamente isso que os R$ 700 de 13/07 fizeram no
            // extrato real — abateram a fatura seguinte, não a de 2025.
            if (ultimaFechada is not null && ultimaFechada.Resumo.SaldoRestante > 0.02m)
            {
                var aplicar = Math.Min(restante, ultimaFechada.Resumo.SaldoRestante);
                ultimaFechada.Resumo.Pagamentos.Add(new PagamentoAplicado(pagamento.Data, aplicar));
                restante -= aplicar;
            }

            if (restante <= 0.02m) continue;

            // Ciclo em curso na data do pagamento: o primeiro que ainda não
            // tinha fechado. É nele que o adiantamento entra.
            var emCurso = ciclos
                .Where(c => c.Fechamento > pagamento.Data.Date)
                .OrderBy(c => c.Fechamento)
                .FirstOrDefault();

            emCurso?.Resumo.Adiantamentos.Add(new PagamentoAplicado(pagamento.Data, restante));
        }
    }

    public static (string Texto, string Classe) BadgeStatusPagamento(string status) => status switch
    {
        "Paga" => ("Paga", "text-bg-success"),
        "Parcialmente paga" => ("Parcialmente paga", "text-bg-warning"),
        "Em aberto" => ("Em aberto", "text-bg-light border text-secondary"),
        _ => ("Sem confirmação de pagamento", "text-bg-light border text-secondary")
    };
}
