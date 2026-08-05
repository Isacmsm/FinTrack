using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinTrack.Filters;

// Mesma ideia do VerificaSessaoFilter, mas pra sessão de operador
// (IdOperador), separada da sessão de usuário do app (IdUser). Aplicado só
// em /Admin — /AdminAcesso (login) fica de fora, senão o próprio login
// entraria em loop de redirect.
public class VerificaSessaoOperadorFilter : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var idOperador = context.HttpContext.Session.GetInt32("IdOperador");

        if (idOperador == null)
        {
            context.Result = new RedirectToPageResult("/AdminAcesso/Login");
            return;
        }

        await next();
    }
}
