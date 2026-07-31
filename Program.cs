using FinTrack.Data;
using FinTrack.Filters;
using FinTrack.Models;
using Microsoft.AspNetCore.DataProtection;
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
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
