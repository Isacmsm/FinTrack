using System;

namespace FinTrack.Models;

/// <summary>
/// Meta mensal fixa por categoria (não uma coluna por mês, como na planilha
/// Orçamento_Conquer — pra variação mês a mês já existe Recorrente).
/// </summary>
public partial class Orcamento
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public int IdCategoria { get; set; }

    public decimal Valor { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
