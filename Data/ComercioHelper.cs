using System.Text.RegularExpressions;

namespace FinTrack.Data;

/// <summary>
/// Deriva uma chave de comércio a partir do merchant da Pluggy (quando presente)
/// ou de heurísticas sobre a descrição — usada tanto pra recategorização em
/// massa em Transações quanto pra herdar categoria em novas importações.
/// Ex.: "Compra no débito via NuPay|iFood" e "Estorno de Uber - NuPay" viram
/// "IFOOD" e "UBER", batendo com as transações já categorizadas desses comércios.
/// </summary>
public static partial class ComercioHelper
{
    [GeneratedRegex(@"^[A-Za-z]{2,6}\*")]
    private static partial Regex PrefixoGatewayRegex();

    [GeneratedRegex(@"^Estorno de\s+", RegexOptions.IgnoreCase)]
    private static partial Regex PrefixoEstornoRegex();

    [GeneratedRegex(@"\s*-\s*(NuPay|Débito|Crédito)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex SufixoCanalRegex();

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
        texto = PrefixoGatewayRegex().Replace(texto, "");

        return texto.Trim().ToUpperInvariant();
    }
}
