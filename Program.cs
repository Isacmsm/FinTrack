using FinTrack.Data;
using FinTrack.Filters;
using FinTrack.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Lax, não Strict: já bloqueia POST cross-site (que é o que interessa
    // aqui) sem quebrar quem volta pro app por um link externo. Strict faria
    // a pessoa parecer deslogada ao chegar de fora, mesmo com sessão válida.
    options.Cookie.SameSite = SameSiteMode.Lax;

    // Em produção o cookie de sessão só trafega por HTTPS. Em dev fica
    // liberado porque o `dotnet run` também serve HTTP puro.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;

    // O app manda heartbeat a cada 15s (ver Painel Admin no CLAUDE.md), então
    // uma aba aberta nunca expira sozinha; esse prazo vale pra quem fecha o
    // navegador e volta depois — e pro /admin, que não tem heartbeat.
    options.IdleTimeout = TimeSpan.FromHours(4);
});

// Login é o único lugar do app onde tentar de novo em massa compensa, e o que
// está atrás do /admin é o log com dado bancário de todo mundo. Janela fixa
// por IP: 8 tentativas a cada 5 minutos, o suficiente pra quem errou a senha
// e inviável pra força bruta.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", contexto =>
        // Só o POST entra na conta — senão recarregar a página de login
        // gastaria as tentativas de quem nem tentou entrar ainda.
        HttpMethods.IsPost(contexto.Request.Method)
            ? RateLimitPartition.GetFixedWindowLimiter(
                contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 8,
                    Window = TimeSpan.FromMinutes(5),
                    QueueLimit = 0
                })
            : RateLimitPartition.GetNoLimiter("get"));

    // 429 com corpo JSON: o site.js lê a mensagem igual faz com os 400 dos
    // handlers (ver Error Handling no CLAUDE.md).
    options.OnRejected = async (contexto, cancelamento) =>
    {
        contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        contexto.HttpContext.Response.ContentType = "application/json";
        await contexto.HttpContext.Response.WriteAsync(
            """{"mensagem":"Muitas tentativas de login. Espere alguns minutos e tente de novo."}""",
            cancelamento);
    };
});

builder.Services.AddDbContext<FinTrackDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinTrack")));

// Hash de senha: implementação do próprio ASP.NET Core (PBKDF2 com salt por
// usuário). Não requer pacote extra além do Identity.Core, que já vem no
// framework compartilhado.
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Operador (login do painel /admin) é uma entidade separada de Usuario —
// ver Models/Operador.cs.
builder.Services.AddSingleton<IPasswordHasher<Operador>, PasswordHasher<Operador>>();

// Painel /admin. Só o PresenceTracker fica em memória, e ele é minúsculo e
// autolimpante (ver Data/PresenceTracker.cs) — "quem está online agora" não
// tem como vir de disco. Log é lido do arquivo a cada tela; não existe buffer
// em memória.
builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddSingleton<AppStatus>();
builder.Services.AddSingleton<LogFileStore>();

// Substitui o logger padrão só pra ganhar o sink de arquivo com retenção —
// tudo continua indo pros mesmos LogLevel de appsettings.json.
builder.Host.UseSerilog((context, services, configuration) =>
{
    var pastaLogs = Path.Combine(context.HostingEnvironment.ContentRootPath, "logs");

    configuration
        // Mesmos níveis que estavam em appsettings.json:Logging (schema do
        // provider padrão, que o Serilog não lê sozinho).
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        // Sem isso o arquivo do dia enche de "Executed DbCommand" com o SQL
        // inteiro, e os erros de verdade (os dois LogInformation/LogError de
        // Util.cs) somem no meio.
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(pastaLogs, "fintrack-.log"),
            rollingInterval: RollingInterval.Day,
            // Tempo, não contagem: com 1 arquivo/dia os dois coincidem, mas é
            // o parâmetro certo pro que foi pedido ("apagar depois de 15
            // dias") em vez de depender dessa coincidência.
            retainedFileTimeLimit: TimeSpan.FromDays(15));

    // O que a Pluggy devolve NÃO passa por aqui. Vai direto pro arquivo do
    // usuário, escrito por Data/PluggyLogSessao.cs — assim não existe filtro
    // pra dar errado e deixar dado bancário vazar pro console ou pro log de
    // 15 dias.
});

// Data Protection: usada pra proteger o Client ID/Secret do Meu Pluggy de
// cada usuário (diferente do hash de senha, aqui precisa dar pra reverter,
// já que o valor em claro é necessário pra chamar a API da Pluggy). Sem
// PersistKeysToFileSystem as chaves são efêmeras e um restart tornaria os
// segredos já gravados ilegíveis pra sempre.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

// Primeira integração HTTP de saída do projeto: API da Pluggy.
builder.Services.AddHttpClient("Pluggy", cliente =>
{
    cliente.BaseAddress = new Uri("https://api.pluggy.ai");
});
builder.Services.AddScoped<PluggyApiClient>();

// Scoped: uma sincronização por requisição, um arquivo por usuário.
builder.Services.AddScoped<PluggyLogSessao>();

var razorPages = builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddFolderApplicationModelConvention(
        "/App",
        model => model.Filters.Add(new VerificaSessaoFilter())
    );

    // /AdminAcesso (login/logout de operador) fica de fora de propósito —
    // ver Filters/VerificaSessaoOperadorFilter.cs.
    options.Conventions.AddFolderApplicationModelConvention(
        "/Admin",
        model => model.Filters.Add(new VerificaSessaoOperadorFilter())
    );
});

// Só em desenvolvimento: recompilar .cshtml em runtime custa carregar o
// Roslyn e manter o compilador vivo dentro do container. Em produção o .cshtml
// já vem compilado no publish, e editar arquivo no servidor não é fluxo deste
// projeto (o deploy é build + up -d).
if (builder.Environment.IsDevelopment())
{
    razorPages.AddRazorRuntimeCompilation();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    // Atrás do Caddy: o proxy termina o HTTPS e repassa HTTP puro pro
    // container, então sem isso UseHttpsRedirection() entraria em loop (o
    // app nunca veria a requisição como https). KnownNetworks/KnownProxies
    // vazios porque o Caddy chega pela rede interna do compose, não do
    // loopback que o middleware confia por padrão — seguro aqui porque a
    // porta do app não é exposta fora da rede do Docker.
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        KnownIPNetworks = { },
        KnownProxies = { }
    });

    // Só em produção: o banco do VPS nasce vazio, e não há passo manual de
    // deploy que rode "dotnet ef database update" (o publish não carrega as
    // ferramentas do EF). Em dev o fluxo continua manual, como documentado
    // no CLAUDE.md.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<FinTrackDbContext>().Database.Migrate();
}

// Bootstrap do primeiro Operador: não existe página de registro pro painel
// /admin de propósito (ver Models/Operador.cs), então a única conta nasce
// daqui, a partir de "Admin:Email"/"Admin:SenhaInicial" na configuração. Só
// roda se a tabela estiver vazia e a configuração estiver presente — sem ela,
// não mexe no banco (importante em dev, onde a migration do Operador pode
// ainda não ter sido aplicada).
var emailAdmin = builder.Configuration["Admin:Email"];
var senhaInicialAdmin = builder.Configuration["Admin:SenhaInicial"];

if (!string.IsNullOrWhiteSpace(emailAdmin) && !string.IsNullOrWhiteSpace(senhaInicialAdmin))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();

    if (!await db.Operadores.AnyAsync())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Operador>>();
        var operador = new Operador { Nome = "Admin", Email = emailAdmin };
        operador.Senha = hasher.HashPassword(operador, senhaInicialAdmin);

        db.Operadores.Add(operador);
        await db.SaveChangesAsync();
    }
}

// Sessões que ficaram abertas (FimEm == null) na última execução: como
// AddSession() aqui é em memória (sem store distribuído), um restart já
// derruba toda sessão de qualquer forma, e o PresenceTracker some junto —
// sem isso, essas linhas nunca mais fechariam (Pages/Admin/Index.cshtml só
// sabe fechar comparando com o PresenceTracker atual). Motivo "restart" pra
// diferenciar de um logout de verdade quando alguém for ler o histórico.
try
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<FinTrackDbContext>().SessoesUsuario
        .Where(s => s.FimEm == null)
        .ExecuteUpdateAsync(s => s
            .SetProperty(x => x.FimEm, DateTime.UtcNow)
            .SetProperty(x => x.Motivo, "restart"));
}
catch (Exception ex)
{
    // Não impede o app de subir. Em dev a migration do SessaoUsuario pode
    // ainda não ter sido aplicada, e mesmo em produção derrubar o boot inteiro
    // por causa de uma faxina de histórico seria desproporcional — sem contar
    // que é justamente com o app no ar que o /admin mostra o que houve.
    app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("FinTrack.Boot")
        .LogWarning(ex, "Não foi possível fechar as sessões que ficaram abertas na execução anterior");
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
