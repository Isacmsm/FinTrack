using System;

namespace FinTrack.Data;

/// <summary>
/// Conversão de instante gravado (sempre UTC) para o horário de quem lê.
///
/// <para>
/// A regra do projeto é: <b>grava em UTC, exibe em local</b>. Quem grava usa
/// <c>DateTime.UtcNow</c>; quem exibe passa por aqui. Sem isso o painel /admin
/// e a página do Pluggy mostravam o UTC formatado como se fosse horário de
/// Brasília — três horas adiantado, sem nada na tela indicando isso.
/// </para>
///
/// <para>
/// O fuso vem do sistema (<see cref="TimeZoneInfo.Local"/>), não de uma
/// constante: em produção o container declara <c>TZ=America/Sao_Paulo</c> no
/// docker-compose.yml, e na máquina de desenvolvimento vale o fuso do próprio
/// SO. Fixar "America/Sao_Paulo" no código quebraria para qualquer pessoa que
/// rodasse o FinTrack em outro fuso.
/// </para>
///
/// <para>
/// Não confundir com <c>DateTime.Today</c>/<c>DateTime.Now</c>, que já são
/// locais e são usados para "que mês é este" (dashboard, filtros, ciclo de
/// fatura). Esses dependem do <c>TZ</c> estar certo — em UTC, o mês virava
/// três horas antes da meia-noite de Brasília.
/// </para>
/// </summary>
public static class Fuso
{
    public static DateTime ParaLocal(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();

    public static DateTime? ParaLocal(DateTime? utc) =>
        utc is null ? null : ParaLocal(utc.Value);

    /// <summary>Formato padrão do painel: dia/mês hora:minuto, já no fuso local.</summary>
    public static string Curto(DateTime utc) => ParaLocal(utc).ToString("dd/MM HH:mm");

    public static string Curto(DateTime? utc) => utc is null ? "—" : Curto(utc.Value);
}
