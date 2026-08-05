using System;
using System.Collections.Generic;

namespace FinTrack.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public virtual ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();

    public virtual ICollection<Divida> Dividas { get; set; } = new List<Divida>();

    public virtual ICollection<Recorrente> Recorrentes { get; set; } = new List<Recorrente>();

    public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

    public virtual PluggyConexao? PluggyConexao { get; set; }

    public virtual ICollection<PluggyItem> PluggyItems { get; set; } = new List<PluggyItem>();

    public virtual ICollection<PluggyFatura> PluggyFaturas { get; set; } = new List<PluggyFatura>();

    public virtual ICollection<PluggyInvestimento> PluggyInvestimentos { get; set; } = new List<PluggyInvestimento>();

    public virtual ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();

    public virtual ICollection<MetaInvestimento> Metas { get; set; } = new List<MetaInvestimento>();

    public virtual ICollection<SessaoUsuario> Sessoes { get; set; } = new List<SessaoUsuario>();

    public virtual ICollection<PluggyFaturaPagamento> PluggyFaturaPagamentos { get; set; } = new List<PluggyFaturaPagamento>();
}
