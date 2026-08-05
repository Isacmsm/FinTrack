using Microsoft.AspNetCore.DataProtection;

namespace FinTrack.Data;

/// <summary>
/// Bilhete de uso único-ish que carrega "quem acabou de provar a senha" entre
/// a requisição que valida o login e a que estabelece a sessão.
///
/// <para>
/// <b>Por que existe:</b> <c>AddSession()</c> não tem API pública pra trocar o
/// id da sessão. Sem trocar, um cookie de sessão plantado no navegador da
/// vítima antes do login continua valendo depois dela entrar — session
/// fixation. A saída é fazer o login em dois passos: o POST valida a senha,
/// <b>apaga o cookie de sessão</b> e devolve este bilhete; o GET seguinte
/// chega sem cookie nenhum, o middleware cria uma sessão nova em branco, e é
/// nela que a identidade é gravada. O id muda porque a sessão é outra.
/// </para>
///
/// <para>
/// <b>Nada em memória e nada no banco:</b> o bilhete é o próprio id cifrado
/// com Data Protection e prazo de validade — o servidor não guarda lista de
/// bilhetes pendentes. Dura 30 segundos, o tempo de um redirect; roubar a URL
/// nessa janela exige já estar lendo o tráfego HTTPS da pessoa.
/// </para>
///
/// <para>
/// Propósitos diferentes para app e painel: um bilhete de usuário não
/// serve para entrar no /admin, nem o contrário — a Data Protection recusa
/// desproteger o que foi protegido com outro propósito.
/// </para>
/// </summary>
public static class TicketLogin
{
    public const string PropositoUsuario = "FinTrack.Login.Usuario";
    public const string PropositoOperador = "FinTrack.Login.Operador";

    private static readonly TimeSpan Validade = TimeSpan.FromSeconds(30);

    /// <summary>
    /// O que já estava na sessão e precisa sobreviver à troca.
    ///
    /// <para>
    /// Existe porque as duas identidades convivem no mesmo navegador de
    /// propósito (dá pra estar logado no app e no /admin ao mesmo tempo) — e a
    /// sessão nova nasce em branco. Sem carregar isso no bilhete, entrar no
    /// /admin derrubava a sessão do app e vice-versa.
    /// </para>
    /// </summary>
    public sealed record SessaoPreservada
    {
        public int? IdUser { get; init; }
        public string? NomeUser { get; init; }
        public string? EmailUser { get; init; }
        public string? TokenSessao { get; init; }
        public int? IdOperador { get; init; }
        public string? NomeOperador { get; init; }
    }

    public sealed record ConteudoTicket(int Id, SessaoPreservada Preservada);

    private static ITimeLimitedDataProtector Protetor(IDataProtectionProvider provider, string proposito) =>
        provider.CreateProtector(proposito).ToTimeLimitedDataProtector();

    /// <summary>
    /// Lê a sessão atual sem escrever nada nela. Ler é seguro aqui: o
    /// middleware só reemite o cookie quando um valor é <b>gravado</b> numa
    /// sessão que ainda não tinha cookie válido.
    /// </summary>
    public static SessaoPreservada CapturarSessao(HttpContext contexto) => new()
    {
        IdUser = contexto.Session.GetInt32("IdUser"),
        NomeUser = contexto.Session.GetString("NomeUser"),
        EmailUser = contexto.Session.GetString("EmailUser"),
        TokenSessao = contexto.Session.GetString("TokenSessao"),
        IdOperador = contexto.Session.GetInt32("IdOperador"),
        NomeOperador = contexto.Session.GetString("NomeOperador")
    };

    /// <summary>
    /// Devolve pra sessão nova o que foi capturado antes da troca. Chame
    /// <b>antes</b> de gravar a identidade recém-autenticada: quem acabou de
    /// entrar tem que sobrescrever o que veio de antes, não o contrário.
    /// </summary>
    public static void RestaurarSessao(HttpContext contexto, SessaoPreservada preservada)
    {
        if (preservada.IdUser is int idUser) contexto.Session.SetInt32("IdUser", idUser);
        if (preservada.NomeUser is { } nomeUser) contexto.Session.SetString("NomeUser", nomeUser);
        if (preservada.EmailUser is { } emailUser) contexto.Session.SetString("EmailUser", emailUser);
        if (preservada.TokenSessao is { } token) contexto.Session.SetString("TokenSessao", token);
        if (preservada.IdOperador is int idOperador) contexto.Session.SetInt32("IdOperador", idOperador);
        if (preservada.NomeOperador is { } nomeOperador) contexto.Session.SetString("NomeOperador", nomeOperador);
    }

    public static string Gerar(IDataProtectionProvider provider, string proposito, int id, SessaoPreservada preservada) =>
        Protetor(provider, proposito).Protect(
            System.Text.Json.JsonSerializer.Serialize(new ConteudoTicket(id, preservada)), Validade);

    /// <summary>Devolve null se o bilhete expirou, foi adulterado ou é de outro propósito.</summary>
    public static ConteudoTicket? Ler(IDataProtectionProvider provider, string proposito, string? bilhete)
    {
        if (string.IsNullOrWhiteSpace(bilhete)) return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<ConteudoTicket>(
                Protetor(provider, proposito).Unprotect(bilhete));
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Apaga o cookie de sessão para que a próxima requisição chegue sem ele e
    /// ganhe uma sessão nova. Só funciona se esta requisição não escrever nada
    /// na sessão — se escrever, o middleware reemite o cookie antigo no fim.
    /// </summary>
    public static void DescartarCookieDeSessao(HttpContext contexto)
    {
        contexto.Response.Cookies.Delete(".AspNetCore.Session", new CookieOptions { Path = "/" });
    }
}
