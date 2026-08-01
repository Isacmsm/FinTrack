using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Data
{
    public static class Util
    {
        /// <summary>
        /// O que o usuário vê quando o erro não é previsto. O detalhe real vai
        /// para o log — nunca para o navegador, porque a mensagem de uma
        /// exceção de infraestrutura (SqlException, timeout, falha de rede)
        /// entrega nome de tabela, coluna e às vezes servidor.
        /// </summary>
        private const string MensagemErroInesperado = "Ocorreu um erro inesperado. Tente novamente.";

        public static IActionResult ExecutarHandler(HttpContext httpContext, Func<IActionResult> acao)
        {
            try
            {
                return acao();
            }
            catch (Exception ex)
            {
                return TratarErro(httpContext, ex);
            }
        }

        /// <summary>
        /// Versão assíncrona: use com os métodos do EF Core (SaveChangesAsync,
        /// FirstOrDefaultAsync...). Chamar .Result num handler bloqueia a thread
        /// e pode travar a aplicação sob carga.
        /// </summary>
        public static async Task<IActionResult> ExecutarHandlerAsync(HttpContext httpContext, Func<Task<IActionResult>> acao)
        {
            try
            {
                return await acao();
            }
            catch (Exception ex)
            {
                return TratarErro(httpContext, ex);
            }
        }

        /// <summary>
        /// Três destinos possíveis, e a diferença entre eles é o que o usuário
        /// pode fazer a respeito:
        ///
        /// <list type="bullet">
        /// <item>Erro de campo -> marca o input, o usuário corrige e reenvia.</item>
        /// <item>Erro de negócio -> faixa vermelha com a mensagem escrita para ele.</item>
        /// <item>Qualquer outro -> faixa vermelha genérica + log com a exceção
        /// inteira. É o único caso em que existe informação que o usuário não
        /// deve ver e que o desenvolvedor precisa ver.</item>
        /// </list>
        ///
        /// O status continua 400 nos três casos: o site.js só sabe ler o corpo
        /// JSON quando o status é 400.
        /// </summary>
        private static IActionResult TratarErro(HttpContext httpContext, Exception ex)
        {
            switch (ex)
            {
                case ErroValidacaoException erroValidacao:
                    return ErroDeValidacao(erroValidacao);

                case ErroDeNegocioException:
                    // Information, não Warning: no fluxo normal isso é só o
                    // usuário esbarrando numa regra. Mas o PluggyApiClient
                    // também lança daqui carregando a resposta crua da API —
                    // sem esse log, uma falha da Pluggy só existiria na tela
                    // de quem estava usando, e some quando a página recarrega.
                    Logger(httpContext).LogInformation("Erro de negócio em {Caminho}: {Mensagem}",
                        httpContext.Request.Path, ex.Message);

                    return new JsonResult(new { mensagem = ex.Message }) { StatusCode = 400 };

                default:
                    Logger(httpContext).LogError(
                        ex,
                        "Erro inesperado no handler {Metodo} {Caminho}{Query}",
                        httpContext.Request.Method,
                        httpContext.Request.Path,
                        httpContext.Request.QueryString);

                    return new JsonResult(new { mensagem = MensagemErroInesperado }) { StatusCode = 400 };
            }
        }

        // Util é estático (chamado direto do @functions das páginas), então o
        // logger vem do container da requisição em vez de construtor.
        private static ILogger Logger(HttpContext httpContext) =>
            httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("FinTrack.Handler");

        // Erros de campo -> o site.js marca cada input individualmente.
        private static JsonResult ErroDeValidacao(ErroValidacaoException ex)
        {
            var errosInput = ex.Erros.Select(e => new
            {
                nomeInput = e.NomeInput,
                mensagem = e.Mensagem
            }).ToList();

            return new JsonResult(new { errosInput }) { StatusCode = 400 };
        }
    }
}
