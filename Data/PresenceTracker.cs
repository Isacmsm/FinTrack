using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FinTrack.Data;

public record PresencaInfo(string Token, int IdUser, string NomeUser, string Caminho, DateTime UltimoPing);

// Em memória, alimentado pelo heartbeat de Pages/App/Heartbeat.cshtml. Não
// sobrevive a restart e não precisa: é "quem tá online agora", não
// histórico — o histórico vive em SessaoUsuario (ver Models/SessaoUsuario.cs).
//
// Chave é o token de sessão (Guid gerado no login), não IdUser: o mesmo
// usuário logado em dois lugares (celular + desktop) tem dois tokens, então
// aparece como duas linhas em "quem tá online" — mais correto do que
// colapsar os dois num só.
public class PresenceTracker
{
    // "Online" = teve heartbeat nos últimos 2x o intervalo do JS (15s) — dá
    // folga pra uma falha de rede isolada sem marcar como offline à toa.
    private static readonly TimeSpan JanelaOnline = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Depois disso a entrada não serve mais pra nada: já saiu da janela de
    /// "online" e o /admin já teve tempo de sobra pra fechar a SessaoUsuario
    /// correspondente com esse último ping (o timeout de lá é de 2 minutos).
    /// </summary>
    private static readonly TimeSpan JanelaRetencao = TimeSpan.FromMinutes(30);

    private readonly ConcurrentDictionary<string, PresencaInfo> _presencas = new();

    public void RegistrarAtividade(string token, int idUser, string nomeUser, string caminho)
    {
        _presencas[token] = new PresencaInfo(token, idUser, nomeUser, caminho, DateTime.UtcNow);

        // Poda no próprio heartbeat: sem isso o dicionário cresce um registro
        // por login pra sempre e só zera no restart. São poucos bytes cada,
        // mas Online() varre tudo a cada render do /admin — e o painel agora
        // recarrega essa lista sozinho a cada poucos segundos.
        var limite = DateTime.UtcNow - JanelaRetencao;
        foreach (var (chave, presenca) in _presencas)
        {
            if (presenca.UltimoPing < limite) _presencas.TryRemove(chave, out _);
        }
    }

    /// <summary>Quantas entradas estão vivas — mostrado no /admin pra dar pra ver se cresce sem parar.</summary>
    public int Rastreadas => _presencas.Count;

    public IReadOnlyList<PresencaInfo> Online()
    {
        var limite = DateTime.UtcNow - JanelaOnline;
        return _presencas.Values
            .Where(p => p.UltimoPing >= limite)
            .OrderByDescending(p => p.UltimoPing)
            .ToList();
    }

    // Último ping conhecido pro token, mesmo que já tenha saído da janela de
    // "online" — usado por Pages/Admin/Index.cshtml pra fechar SessaoUsuario
    // com um FimEm melhor que "agora" quando a sessão já não manda heartbeat
    // há um tempo.
    public DateTime? UltimoPing(string token) =>
        _presencas.TryGetValue(token, out var p) ? p.UltimoPing : null;
}
