using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Data;

/// <summary>
/// Executado na inicialização: aplica as migrations pendentes e cria os perfis
/// e os usuários padrão do sistema.
/// </summary>
public static class DbInitializer
{
    public const string EmailAdministrador = "admin@fitmanager.com";
    public const string SenhaAdministrador = "Admin@123";

    public const string EmailInstrutor = DadosIniciais.EmailInstrutorPadrao;
    public const string SenhaInstrutor = "Instrutor@123";

    public static async Task InicializarAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();

        if (!context.Database.GetMigrations().Any())
        {
            throw new InvalidOperationException(
                "Nenhuma migration foi encontrada no projeto. Na pasta do projeto, execute " +
                "'dotnet ef migrations add CriacaoInicial' e rode a aplicação novamente. " +
                "Veja o passo a passo no README.md.");
        }

        // Cria o banco (se não existir) e aplica as migrations pendentes.
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var perfil in new[] { Perfis.Administrador, Perfis.Instrutor })
        {
            if (!await roleManager.RoleExistsAsync(perfil))
            {
                await roleManager.CreateAsync(new IdentityRole(perfil));
            }
        }

        // Os usuários padrão são criados apenas na primeira execução (banco sem usuários).
        // Depois disso, os logins são mantidos pelo próprio sistema: o administrador cadastra
        // os instrutores e cada usuário altera o e-mail e a senha em "Minha conta".
        if (!await userManager.Users.AnyAsync())
        {
            await CriarUsuarioAsync(userManager, EmailAdministrador, SenhaAdministrador, Perfis.Administrador);
            await CriarUsuarioAsync(userManager, EmailInstrutor, SenhaInstrutor, Perfis.Instrutor);
        }

        await context.EncerrarMatriculasVencidasAsync();
    }

    private static async Task CriarUsuarioAsync(UserManager<IdentityUser> userManager, string email, string senha, string perfil)
    {
        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario is null)
        {
            usuario = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

            var resultado = await userManager.CreateAsync(usuario, senha);
            if (!resultado.Succeeded)
            {
                var erros = string.Join("; ", resultado.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Não foi possível criar o usuário {email}: {erros}");
            }
        }

        if (!await userManager.IsInRoleAsync(usuario, perfil))
        {
            await userManager.AddToRoleAsync(usuario, perfil);
        }
    }
}
