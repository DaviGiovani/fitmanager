using System.Globalization;
using FitManager.Data;
using FitManager.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Banco de dados (SQL Server + Entity Framework Core) ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi encontrada no appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// ---------- Autenticação e perfis (ASP.NET Core Identity) ----------
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<IdentityErrorDescriberPtBr>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AcessoNegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ---------- MVC ----------
builder.Services.AddControllersWithViews(options =>
{
    // Todas as telas exigem usuário autenticado; as exceções usam [AllowAnonymous].
    var politica = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(politica));

    // Proteção contra CSRF em todos os POSTs.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    // Mensagens de erro de preenchimento em português.
    var mensagens = options.ModelBindingMessageProvider;
    mensagens.SetValueMustNotBeNullAccessor(_ => "Este campo é obrigatório.");
    mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
    mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para o campo {campo}.");
    mensagens.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
    mensagens.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
    mensagens.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número.");
    mensagens.SetMissingBindRequiredValueAccessor(campo => $"O campo {campo} é obrigatório.");
    mensagens.SetMissingKeyOrValueAccessor(() => "Este campo é obrigatório.");
});

var app = builder.Build();

// ---------- Cultura pt-BR (datas dd/MM/yyyy e valores com vírgula) ----------
var culturaBrasil = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaBrasil;
CultureInfo.DefaultThreadCurrentUICulture = culturaBrasil;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaBrasil),
    SupportedCultures = new List<CultureInfo> { culturaBrasil },
    SupportedUICultures = new List<CultureInfo> { culturaBrasil }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Erro");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------- Aplica as migrations e cria os dados iniciais ----------
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InicializarAsync(scope.ServiceProvider);
}

app.Run();
