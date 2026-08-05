using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FinTrack.Data;

/// <summary>
/// Cabeçalho de um evento do log do Pluggy — tudo menos o JSON em si.
/// A tela lista cabeçalhos e só busca o conteúdo do que for aberto: um BRUTO
/// de transações passa de 1 MB, e mandar todos de uma vez pro navegador é
/// exatamente o "paredão de texto" que essa página existe pra evitar.
/// </summary>
public record CabecalhoPluggy(int Indice, DateTime Momento, string Tipo, string Operacao, string Chave, string? Nota, int Bytes);

/// <summary>
/// Leitura dos arquivos de log para o painel /admin. Só lê — quem escreve é o
/// Serilog (log geral, em Program.cs) e o <see cref="PluggyLogSessao"/> (log do
/// Pluggy).
///
/// <para>
/// <b>Nada é mantido em memória entre requisições.</b> Não existe buffer: cada
/// tela lê do disco o pedaço que vai mostrar e descarta. Foi uma troca
/// deliberada — o buffer antigo segurava 500 entradas vivas o tempo todo no
/// processo e sumia a cada restart, então custava RAM e ainda perdia
/// justamente o que interessava depois de uma queda.
/// </para>
///
/// <para>
/// Nada de nome de arquivo vindo do cliente: a data é validada (8 dígitos) e o
/// usuário do log do Pluggy é um <c>int</c>. Path traversal não tem por onde
/// entrar.
/// </para>
/// </summary>
public class LogFileStore(IWebHostEnvironment ambiente)
{
    private const string PrefixoGeral = "fintrack-";

    private string PastaLogs => Path.Combine(ambiente.ContentRootPath, "logs");

    // ---------- Log geral da aplicação ----------

    public IReadOnlyList<string> DatasDisponiveis()
    {
        if (!Directory.Exists(PastaLogs)) return [];

        return Directory.GetFiles(PastaLogs, $"{PrefixoGeral}*.log")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(nome => nome![PrefixoGeral.Length..])
            .Where(data => data.Length == 8 && data.All(char.IsDigit))
            .OrderDescending()
            .ToList();
    }

    /// <summary>
    /// Últimas linhas do dia pedido. <c>ReadLines</c> + <c>TakeLast</c> passa o
    /// arquivo em streaming e segura só a janela pedida — um log de 50 MB não
    /// vira 50 MB de RAM.
    /// </summary>
    public IReadOnlyList<string> UltimasLinhas(string data, int quantidade = 500)
    {
        if (data.Length != 8 || !data.All(char.IsDigit)) return [];

        var caminho = Path.Combine(PastaLogs, $"{PrefixoGeral}{data}.log");
        if (!File.Exists(caminho)) return [];

        return File.ReadLines(caminho).TakeLast(quantidade).ToList();
    }

    // ---------- Log do Pluggy (último de cada usuário) ----------

    /// <summary>Ids de usuário que têm log do Pluygy em disco, e quando foi a última busca.</summary>
    public IReadOnlyList<(int IdUser, DateTime ModificadoEm, long Bytes)> UsuariosComLogPluggy()
    {
        var pasta = PluggyLogSessao.PastaLogs(ambiente);
        if (!Directory.Exists(pasta)) return [];

        return Directory.GetFiles(pasta, "usuario-*.ndjson")
            .Select(caminho =>
            {
                var nome = Path.GetFileNameWithoutExtension(caminho);
                var ok = int.TryParse(nome["usuario-".Length..], out var idUser);
                var info = new FileInfo(caminho);
                return (Ok: ok, IdUser: idUser, ModificadoEm: info.LastWriteTimeUtc, Bytes: info.Length);
            })
            .Where(x => x.Ok)
            .Select(x => (x.IdUser, x.ModificadoEm, x.Bytes))
            .OrderByDescending(x => x.ModificadoEm)
            .ToList();
    }

    private const string MarcaConteudo = ",\"conteudo\":";

    /// <summary>
    /// Cabeçalhos da última sincronização do usuário, sem tocar no JSON do
    /// conteúdo: corta a linha antes de <c>"conteudo"</c> (que
    /// <see cref="PluggyLogSessao"/> sempre escreve por último) e parseia só o
    /// pedacinho da frente. Parsear a linha inteira só pra mostrar
    /// "GET /v2/transactions às 14:02" seria parsear megabytes à toa.
    ///
    /// Linha inválida é ignorada em vez de derrubar a tela — o arquivo pode
    /// estar sendo escrito neste exato momento por uma busca em andamento.
    /// </summary>
    public IReadOnlyList<CabecalhoPluggy> CabecalhosPluggy(int idUser)
    {
        var caminho = PluggyLogSessao.CaminhoDoUsuario(ambiente, idUser);
        if (!File.Exists(caminho)) return [];

        var cabecalhos = new List<CabecalhoPluggy>();
        var indice = 0;

        foreach (var linha in File.ReadLines(caminho))
        {
            var atual = indice++;
            if (string.IsNullOrWhiteSpace(linha)) continue;

            var corte = linha.IndexOf(MarcaConteudo, StringComparison.Ordinal);
            var soCabecalho = corte < 0 ? linha : string.Concat(linha.AsSpan(0, corte), "}");

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(soCabecalho);
                var raiz = doc.RootElement;
                cabecalhos.Add(new CabecalhoPluggy(
                    atual,
                    raiz.GetProperty("momento").GetDateTime(),
                    raiz.GetProperty("tipo").GetString() ?? "",
                    raiz.GetProperty("operacao").GetString() ?? "",
                    raiz.GetProperty("chave").GetString() ?? "",
                    raiz.TryGetProperty("nota", out var nota) ? nota.GetString() : null,
                    corte < 0 ? 0 : linha.Length - corte - MarcaConteudo.Length));
            }
            catch (System.Text.Json.JsonException)
            {
                // Linha truncada (busca ainda rodando) — ignora.
            }
        }

        return cabecalhos;
    }

    /// <summary>
    /// O JSON de um evento, buscado sob demanda quando alguém abre aquele
    /// bloco na tela. Relê o arquivo em streaming e devolve uma linha só.
    /// </summary>
    public string? ConteudoPluggy(int idUser, int indice)
    {
        if (indice < 0) return null;

        var caminho = PluggyLogSessao.CaminhoDoUsuario(ambiente, idUser);
        if (!File.Exists(caminho)) return null;

        var atual = 0;
        foreach (var linha in File.ReadLines(caminho))
        {
            if (atual++ != indice) continue;

            var corte = linha.IndexOf(MarcaConteudo, StringComparison.Ordinal);
            if (corte < 0) return null;

            // Tira exatamente UM "}" — o que fecha o envelope do evento. Um
            // TrimEnd('}') comeria também as chaves finais do próprio JSON do
            // conteúdo, que quase sempre termina em "}}".
            var conteudo = linha[(corte + MarcaConteudo.Length)..];
            return conteudo.EndsWith('}') ? conteudo[..^1] : conteudo;
        }

        return null;
    }

    /// <summary>Arquivo cru, pro botão de baixar do painel.</summary>
    public string LerPluggyBruto(int idUser)
    {
        var caminho = PluggyLogSessao.CaminhoDoUsuario(ambiente, idUser);
        return File.Exists(caminho) ? File.ReadAllText(caminho) : "";
    }
}
