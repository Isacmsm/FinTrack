using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace FinTrack.Data;

/// <summary>
/// Grava o que a Pluggy devolve e o que o FinTrack salva, durante uma
/// sincronização. Serviço <b>scoped</b>: vive uma requisição, e a requisição é
/// sempre um "Buscar"/"Atualizar" de um usuário só.
///
/// <para>
/// <b>Fora do pipeline do Serilog de propósito.</b> Antes esse conteúdo era um
/// <c>ILogger</c> com categoria própria, separado do log geral por um filtro —
/// funcionava, mas a garantia de que dado bancário real nunca cai no console
/// nem no arquivo de 15 dias dependia do filtro estar certo. Aqui a garantia é
/// estrutural: este arquivo é escrito por este código e por mais nada.
/// </para>
///
/// <para>
/// <b>Só o último de cada usuário.</b> O arquivo é aberto em
/// <see cref="FileMode.Create"/> a cada <see cref="Iniciar"/>: a sincronização
/// nova apaga a anterior. Não há retenção, não há rotação por dia e não há
/// histórico — o disco ocupado é no máximo um arquivo por usuário, e o
/// conteúdo é o retrato da última busca, que é o que serve pra depurar.
/// </para>
///
/// <para>
/// <b>Nada em memória.</b> Escreve direto no <see cref="StreamWriter"/>, linha
/// a linha, sem acumular nada em lista. O JSON cru da Pluggy (que passa de
/// 1 MB) atravessa como string e vai embora.
/// </para>
///
/// <para>
/// Formato: NDJSON — um objeto JSON por linha. É o que permite a tela do
/// painel mostrar cada evento separado e com o JSON identado, em vez de um
/// paredão de texto corrido.
/// </para>
/// </summary>
public sealed class PluggyLogSessao(IWebHostEnvironment ambiente) : IDisposable
{
    private StreamWriter? _arquivo;

    public static string PastaLogs(IWebHostEnvironment ambiente) =>
        Path.Combine(ambiente.ContentRootPath, "logs", "pluggy");

    public static string CaminhoDoUsuario(IWebHostEnvironment ambiente, int idUser) =>
        Path.Combine(PastaLogs(ambiente), $"usuario-{idUser}.ndjson");

    public void Iniciar(int idUser, string nomeUser)
    {
        Finalizar();

        var pasta = PastaLogs(ambiente);
        Directory.CreateDirectory(pasta);

        // FileMode.Create: trunca o que existia. É aqui que mora o "só o
        // último log de cada usuário".
        var fluxo = new FileStream(CaminhoDoUsuario(ambiente, idUser), FileMode.Create, FileAccess.Write, FileShare.Read);
        _arquivo = new StreamWriter(fluxo, Encoding.UTF8);

        Registrar("INICIO", "sincronização", nomeUser, "null",
            "Início da sincronização. Tudo abaixo é desta busca — a anterior foi apagada.");
    }

    /// <param name="tipo">BRUTO (como a Pluggy devolveu) ou SALVO (o que virou registro local).</param>
    /// <param name="operacao">Ex.: "GET /accounts".</param>
    /// <param name="chave">O id que liga um BRUTO ao SALVO correspondente.</param>
    /// <param name="conteudoJson">JSON já pronto — vem do GetRawText() da resposta ou de um Serialize.</param>
    /// <param name="nota">O que este bloco significa, e o que da Pluggy foi descartado.</param>
    public void Registrar(string tipo, string operacao, string chave, string conteudoJson, string? nota = null)
    {
        if (_arquivo is null) return;

        // Montado à mão, sem serializar um objeto: conteudoJson já é JSON
        // válido e pode ter megabytes — passar por JsonSerializer significaria
        // parsear e reescrever tudo, dobrando a memória usada à toa.
        var linha = new StringBuilder(conteudoJson.Length + 256);
        linha.Append("{\"momento\":").Append(JsonSerializer.Serialize(DateTime.UtcNow))
            .Append(",\"tipo\":").Append(JsonSerializer.Serialize(tipo))
            .Append(",\"operacao\":").Append(JsonSerializer.Serialize(operacao))
            .Append(",\"chave\":").Append(JsonSerializer.Serialize(chave))
            .Append(",\"nota\":").Append(JsonSerializer.Serialize(nota))
            .Append(",\"conteudo\":").Append(SemQuebraDeLinha(conteudoJson))
            .Append('}');

        _arquivo.WriteLine(linha.ToString());
    }

    /// <summary>
    /// Uma linha do arquivo = um evento. Um \n no meio do JSON partiria o
    /// evento em dois e a tela não conseguiria remontar.
    /// </summary>
    private static string SemQuebraDeLinha(string json) =>
        json.Contains('\n') || json.Contains('\r')
            ? json.Replace("\r", "").Replace("\n", " ")
            : json;

    public void Finalizar()
    {
        _arquivo?.Flush();
        _arquivo?.Dispose();
        _arquivo = null;
    }

    public void Dispose() => Finalizar();
}
