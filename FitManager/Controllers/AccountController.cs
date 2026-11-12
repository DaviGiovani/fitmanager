using FitManager.Data;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _context;

    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, AppDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = await _signInManager.PasswordSignInAsync(
            model.Email.Trim(), model.Senha, model.LembrarMe, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        var mensagem = "E-mail ou senha inválidos.";

        if (resultado.IsLockedOut)
        {
            // Instrutor inativado: o bloqueio não tem data para terminar.
            var usuario = await _userManager.FindByNameAsync(model.Email.Trim());
            var fimBloqueio = usuario is null ? null : await _userManager.GetLockoutEndDateAsync(usuario);

            mensagem = fimBloqueio > DateTimeOffset.UtcNow.AddYears(50)
                ? "O seu acesso está desativado. Procure o administrador da academia."
                : "Muitas tentativas inválidas. Aguarde alguns minutos e tente novamente.";
        }

        ModelState.AddModelError(string.Empty, mensagem);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // GET: /Account/MinhaConta  (editar e-mail e senha do usuário logado)
    [HttpGet]
    public async Task<IActionResult> MinhaConta()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new MinhaContaViewModel { Email = usuario.Email ?? string.Empty });
    }

    // POST: /Account/MinhaConta
    [HttpPost]
    public async Task<IActionResult> MinhaConta(MinhaContaViewModel model)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // A senha atual confirma que é o próprio usuário fazendo a alteração.
        if (!await _userManager.CheckPasswordAsync(usuario, model.SenhaAtual))
        {
            ModelState.AddModelError(nameof(model.SenhaAtual), "Senha atual incorreta.");
            return View(model);
        }

        var emailAntigo = usuario.Email ?? string.Empty;
        var emailNovo = model.Email.Trim();
        var mudouEmail = !string.Equals(emailAntigo, emailNovo, StringComparison.OrdinalIgnoreCase);
        var mudouSenha = !string.IsNullOrEmpty(model.NovaSenha);

        if (!mudouEmail && !mudouSenha)
        {
            TempData["Aviso"] = "Nenhuma alteração foi feita.";
            return RedirectToAction(nameof(MinhaConta));
        }

        // Valida tudo antes de gravar, para não salvar só metade das alterações.
        if (mudouEmail)
        {
            var outro = await _userManager.FindByEmailAsync(emailNovo);
            if (outro is not null && outro.Id != usuario.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Este e-mail já está em uso por outro usuário.");
            }
        }

        if (mudouSenha)
        {
            foreach (var validador in _userManager.PasswordValidators)
            {
                var resultado = await validador.ValidateAsync(_userManager, usuario, model.NovaSenha);
                foreach (var erro in resultado.Errors)
                {
                    ModelState.AddModelError(nameof(model.NovaSenha), erro.Description);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (mudouEmail)
        {
            // O e-mail também é o nome de usuário usado no login.
            usuario.Email = emailNovo;
            usuario.UserName = emailNovo;

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                {
                    ModelState.AddModelError(nameof(model.Email), erro.Description);
                }

                return View(model);
            }

            // O instrutor é ligado ao usuário pelo e-mail; mantém o vínculo.
            var instrutor = await _context.Instrutores.FirstOrDefaultAsync(i => i.Email == emailAntigo);
            if (instrutor is not null)
            {
                instrutor.Email = emailNovo;
                await _context.SaveChangesAsync();
            }
        }

        if (mudouSenha)
        {
            var resultado = await _userManager.ChangePasswordAsync(usuario, model.SenhaAtual, model.NovaSenha!);
            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                {
                    ModelState.AddModelError(nameof(model.NovaSenha), erro.Description);
                }

                return View(model);
            }
        }

        // Atualiza o cookie de login com os novos dados.
        await _signInManager.RefreshSignInAsync(usuario);

        TempData["Sucesso"] = mudouEmail && mudouSenha ? "E-mail e senha atualizados com sucesso."
            : mudouEmail ? "E-mail atualizado com sucesso. Use o novo e-mail no próximo login."
            : "Senha atualizada com sucesso.";

        return RedirectToAction(nameof(MinhaConta));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AcessoNegado()
    {
        return View();
    }
}
