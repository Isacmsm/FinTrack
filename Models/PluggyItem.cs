using System;

namespace FinTrack.Models;

public partial class PluggyItem
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public string ItemId { get; set; } = null!;

    /// <summary>
    /// Apesar do nome, guarda um resumo das CONTAS dentro do Item (ex.:
    /// "Nu Pagamentos S.A. - Instituição de Pagamento, gold"), não o nome do
    /// conector — todo Item do Meu Pluggy usa o mesmo conector "MeuPluggy",
    /// então ele não distingue um banco do outro quando há mais de um; as
    /// contas de fato, sim.
    /// </summary>
    public string? NomeConector { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// connector.imageUrl da Pluggy. Só é distinto por banco quando o Item
    /// nasceu do widget Pluggy Connect (um connector real por instituição);
    /// Items registrados via meu.pluggy.ai usam todos o mesmo connector
    /// "MeuPluggy", então o ícone vem igual pra todos esses.
    /// </summary>
    public string? IconeUrl { get; set; }

    public DateTime? UltimaSincronizacao { get; set; }

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
