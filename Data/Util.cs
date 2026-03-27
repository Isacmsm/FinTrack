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
            catch (ErroExecucaoException ex)
            {
                var errosInput = ex.Erros.Select(e => new
                {
                    nomeInput = e.NomeInput,
                    mensagem = e.Mensagem
                }).ToList();

                return new JsonResult(new { errosInput }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { mensagem = ex.Message }) { StatusCode = 400 };
            }
        }
    }
}