using System.Text.RegularExpressions;

namespace FinTrack.Data;

/// <summary>
/// Deriva uma chave de comércio a partir do merchant da Pluggy (quando presente)
/// ou de heurísticas sobre a descrição — usada tanto pra recategorização em
/// massa em Transações quanto pra herdar categoria em novas importações.
/// Ex.: "Compra no débito via NuPay|iFood" e "Estorno de Uber - NuPay" viram
/// "IFOOD" e "UBER", batendo com as transações já categorizadas desses comércios.
///
/// Pra "Transferência Recebida|Fulano de Tal" o texto depois do "|" é o nome
/// de quem mandou/recebeu — mantém isso como chave (não junta com
/// "Transferência Recebida" sem nome) porque na prática o usuário categoriza
/// diferente por remetente (ex.: salário sem nome vs. reembolso de uma pessoa
/// específica), então agrupar só pelo prefixo genérico juntaria coisas que ele
/// trata como categorias diferentes.
/// </summary>
public static partial class ComercioHelper
{
    [GeneratedRegex(@"^[A-Za-z]{2,6}\*")]
    private static partial Regex PrefixoGatewayRegex();

    [GeneratedRegex(@"^Estorno de\s+", RegexOptions.IgnoreCase)]
    private static partial Regex PrefixoEstornoRegex();

    [GeneratedRegex(@"\s*-\s*(NuPay|Débito|Crédito)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex SufixoCanalRegex();

    // "Pag*Steam 2/3", "Ncopay *Atn - Mais Bel 3/3" — número da parcela no
    // fim da descrição, senão cada parcela vira um "comércio" diferente.
    [GeneratedRegex(@"\s+\d{1,2}/\d{1,2}$")]
    private static partial Regex SufixoParcelaRegex();

    public static string ChaveComercio(string? merchantNome, string? desc)
    {
        if (!string.IsNullOrWhiteSpace(merchantNome))
            return merchantNome.Trim().ToUpperInvariant();

        var texto = desc ?? "";

        var indicePipe = texto.LastIndexOf('|');
        if (indicePipe >= 0 && indicePipe < texto.Length - 1)
            texto = texto[(indicePipe + 1)..];

        texto = PrefixoEstornoRegex().Replace(texto, "");
        texto = SufixoCanalRegex().Replace(texto, "");
        texto = SufixoParcelaRegex().Replace(texto, "");
        texto = PrefixoGatewayRegex().Replace(texto, "");

        return texto.Trim().ToUpperInvariant();
    }
}
