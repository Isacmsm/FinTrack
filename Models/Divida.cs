using System;
using System.Collections.Generic;

namespace FinTrack.Models;

public partial class Divida
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public int? IdBanco { get; set; }

    public string Descricao { get; set; } = null!;

    public decimal ValorTotal { get; set; }

    public decimal? TaxaJuro { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly? DataFinalEstimada { get; set; }

    public DateOnly? DataFinal { get; set; }

    public bool Pagando { get; set; }

    public decimal? ValorParcela { get; set; }

    public int? QntParcelas { get; set; }

    public int QntParcelasPagas { get; set; }

    public decimal? ValorVista { get; set; }

    public int? IdRecorrente { get; set; }

    public virtual Banco? IdBancoNavigation { get; set; }

    public virtual Recorrente? IdRecorrenteNavigation { get; set; }

    public virtual Usuario IdUserNavigation { get; set; } = null!;
}
