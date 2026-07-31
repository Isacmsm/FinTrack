using System;
using System.Collections.Generic;

namespace FinTrack.Models;

public partial class Categoria
{
    public int Id { get; set; }

    public bool Tipo { get; set; }

    public string Nome { get; set; } = null!;

    public int? IdUser { get; set; }

    /// <summary>
    /// Movimentação interna (ex.: aplicação/resgate de investimento) — conta
    /// pra listagem e histórico, mas fica de fora dos totais de receita/despesa
    /// do dashboard e da tela de Transações, pra não inflar os números com
    /// dinheiro que só mudou de lugar dentro das contas do próprio usuário.
    /// </summary>
    public bool EhMovimentacaoInterna { get; set; }

    public virtual Usuario? IdUserNavigation { get; set; }

    public virtual ICollection<Recorrente> Recorrentes { get; set; } = new List<Recorrente>();

    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

    public virtual ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();
}
