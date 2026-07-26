using FinTrack.Data;
using FinTrack.Filters;
using FinTrack.Models;
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
