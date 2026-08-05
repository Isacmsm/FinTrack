using FinTrack.Data;
using FinTrack.Filters;
using FinTrack.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSession();

builder.Services.AddDbContext<FinTrackDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinTrack")));

// Hash de senha: implementação do próprio ASP.NET Core (PBKDF2 com salt por
// usuário). Não requer pacote extra além do Identity.Core, que já vem no
// framework compartilhado.
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

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

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddFolderApplicationModelConvention(
        "/App",
        model => model.Filters.Add(new VerificaSessaoFilter())
    );
}).AddRazorRuntimeCompilation();

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

app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
