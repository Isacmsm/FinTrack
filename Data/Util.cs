using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Data
{
    public static class Util
    {
        public static IActionResult ExecutarHandler(Func<IActionResult> acao)
        {
            try
            {
                return acao();
            }
            catch (ErroValidacaoException ex)
            {
                return ErroDeValidacao(ex);
            }
            catch (Exception ex)
            {
                // Erro geral -> o site.js mostra na faixa vermelha (#erroGeral).
                return new JsonResult(new { mensagem = ex.Message }) { StatusCode = 400 };
            }
        }

        /// <summary>
        /// Versão assíncrona: use com os métodos do EF Core (SaveChangesAsync,
        /// FirstOrDefaultAsync...). Chamar .Result num handler bloqueia a thread
        /// e pode travar a aplicação sob carga.
        /// </summary>
        public static async Task<IActionResult> ExecutarHandlerAsync(Func<Task<IActionResult>> acao)
        {
            try
            {
                return await acao();
            }
            catch (ErroValidacaoException ex)
            {
                return ErroDeValidacao(ex);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { mensagem = ex.Message }) { StatusCode = 400 };
            }
        }

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
