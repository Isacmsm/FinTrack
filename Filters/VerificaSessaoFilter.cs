using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinTrack.Filters;

public class VerificaSessaoFilter : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var idUser = context.HttpContext.Session.GetInt32("IdUser");

        if (idUser == null)
        {
            context.Result = new RedirectToPageResult("/ControleAcesso/Index");
            return;
        }

        await next();
    }
}