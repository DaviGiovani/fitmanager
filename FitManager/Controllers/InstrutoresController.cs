using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

/// <summary>
/// UC06 – Manter Instrutores (RF11). Ao cadastrar um instrutor, o sistema cria o login
/// dele com o perfil Instrutor. O instrutor é ligado ao login pelo e-mail.
/// </summary>
[Authorize(Roles = Perfis.Administrador)]
public class InstrutoresController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public InstrutoresController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Instrutores
    public async Task<IActionResult> Index()
    {
        var instrutores = await _context.Instrutores
            .AsNoTracking()
            .OrderByDescending(i => i.Ativo)
            .ThenBy(i => i.Nome)
            .ToListAsync();

        ViewBag.FichasAtivas = await _context.Treinos
            .Where(t => t.Ativo)
            .GroupBy(t => t.InstrutorId)
            .Select(g => new { InstrutorId = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.InstrutorId, x => x.Quantidade);

        var usuarios = await _userManager.GetUsersInRoleAsync(Perfis.Instrutor);
        ViewBag.EmailsComAcesso = usuarios
            .Where(u => u.Email is not null)
            .Select(u => u.Email!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return View(instrutores);
    }

    // GET: /Instrutores/Create
    public IActionResult Create()
    {
        return View(new InstrutorFormViewModel());
    }

    // POST: /Instrutores/Create
    [HttpPost]
    public async Task<IActionResult> Create(InstrutorFormViewModel model)
    {
        model.Id = 0;
        model.Email = model.Email?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(model.Senha))
        {
            ModelState.AddModelError(nameof(model.Senha), "Informe a senha inicial de acesso do instrutor.");
        }

        await ValidarAsync(model, usuarioAtual: null);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // O login e o instrutor são gravados juntos: se um falhar, nada é salvo.
        await using var transacao = await _context.Database.BeginTransactionAsync();

        var usuario = await CriarLoginAsync(model.Email, model.Senha!);
        if (usuario is null)
        {
            return View(model);
        }

        if (!model.Ativo)
        {
            await DesativarLoginAsync(usuario);
        }

        var instrutor = new Instrutor
        {
            Nome = model.Nome.Trim(),
            Cref = model.Cref.Trim(),
            Telefone = model.Telefone,
            Email = model.Email,
            Ativo = model.Ativo
        };

        _context.Instrutores.Add(instrutor);
        await _context.SaveChangesAsync();
        await transacao.CommitAsync();

        TempData["Sucesso"] = $"Instrutor {instrutor.Nome} cadastrado. O login dele é {instrutor.Email}.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Instrutores/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var instrutor = await _context.Instrutores.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (instrutor is null)
        {
            return NotFound();
        }

        return View(new InstrutorFormViewModel
        {
            Id = instrutor.Id,
            Nome = instrutor.Nome,
            Cref = instrutor.Cref,
            Telefone = instrutor.Telefone,
            Email = instrutor.Email ?? string.Empty,
            Ativo = instrutor.Ativo,
            PossuiAcesso = await BuscarLoginAsync(instrutor.Email) is not null
        });
    }

    // POST: /Instrutores/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, InstrutorFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var instrutor = await _context.Instrutores.FirstOrDefaultAsync(i => i.Id == id);
        if (instrutor is null)
        {
            return NotFound();
        }

        model.Email = model.Email?.Trim() ?? string.Empty;

        var usuario = await BuscarLoginAsync(instrutor.Email);
        model.PossuiAcesso = usuario is not null;

        await ValidarAsync(model, usuario);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var transacao = await _context.Database.BeginTransactionAsync();

        if (usuario is not null)
        {
            // Mantém o login igual ao e-mail do instrutor.
            if (!string.Equals(usuario.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                usuario.Email = model.Email;
                usuario.UserName = model.Email;

                var resultado = await _userManager.UpdateAsync(usuario);
                if (!AdicionarErros(resultado, nameof(model.Email)))
                {
                    return View(model);
                }
            }

            // Redefine a senha, quando informada.
            if (!string.IsNullOrEmpty(model.Senha))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var resultado = await _userManager.ResetPasswordAsync(usuario, token, model.Senha);
                if (!AdicionarErros(resultado, nameof(model.Senha)))
                {
                    return View(model);
                }
            }
        }
        else if (!string.IsNullOrEmpty(model.Senha))
        {
            // Instrutor ainda sem login: cria o acesso agora.
            usuario = await CriarLoginAsync(model.Email, model.Senha);
            if (usuario is null)
            {
                return View(model);
            }
        }

        if (usuario is not null)
        {
            if (model.Ativo)
            {
                await _userManager.SetLockoutEndDateAsync(usuario, null);
            }
            else if (instrutor.Ativo || !await _userManager.IsLockedOutAsync(usuario))
            {
                await DesativarLoginAsync(usuario);
            }
        }

        instrutor.Nome = model.Nome.Trim();
        instrutor.Cref = model.Cref.Trim();
        instrutor.Telefone = model.Telefone;
        instrutor.Email = model.Email;
        instrutor.Ativo = model.Ativo;

        await _context.SaveChangesAsync();
        await transacao.CommitAsync();

        TempData["Sucesso"] = $"Dados de {instrutor.Nome} atualizados com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Instrutores/Delete/5  (tela de confirmação - RNF04)
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var instrutor = await _context.Instrutores.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (instrutor is null)
        {
            return NotFound();
        }

        ViewBag.QuantidadeFichas = await _context.Treinos.CountAsync(t => t.InstrutorId == id);
        ViewBag.PossuiAcesso = await BuscarLoginAsync(instrutor.Email) is not null;
        return View(instrutor);
    }

    // POST: /Instrutores/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var instrutor = await _context.Instrutores.FirstOrDefaultAsync(i => i.Id == id);
        if (instrutor is null)
        {
            return NotFound();
        }

        var usuario = await BuscarLoginAsync(instrutor.Email);
        var possuiFichas = await _context.Treinos.AnyAsync(t => t.InstrutorId == id);

        await using var transacao = await _context.Database.BeginTransactionAsync();

        if (possuiFichas)
        {
            // RN09: instrutor com fichas de treino é inativado, preservando o histórico.
            instrutor.Ativo = false;
            if (usuario is not null)
            {
                await DesativarLoginAsync(usuario);
            }

            await _context.SaveChangesAsync();
            await transacao.CommitAsync();

            TempData["Aviso"] = $"{instrutor.Nome} possui fichas de treino e por isso foi inativado, preservando o histórico. O acesso dele ao sistema foi bloqueado.";
        }
        else
        {
            _context.Instrutores.Remove(instrutor);
            await _context.SaveChangesAsync();

            if (usuario is not null)
            {
                await _userManager.DeleteAsync(usuario);
            }

            await transacao.CommitAsync();

            TempData["Sucesso"] = $"Instrutor {instrutor.Nome} excluído com sucesso, junto com o login dele.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ---------- Métodos auxiliares ----------

    /// <summary>Login com perfil Instrutor ligado ao e-mail informado (ou null).</summary>
    private async Task<IdentityUser?> BuscarLoginAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var usuario = await _userManager.FindByEmailAsync(email);
        return usuario is not null && await _userManager.IsInRoleAsync(usuario, Perfis.Instrutor)
            ? usuario
            : null;
    }

    private async Task ValidarAsync(InstrutorFormViewModel model, IdentityUser? usuarioAtual)
    {
        if (!string.IsNullOrWhiteSpace(model.Cref))
        {
            var cref = model.Cref.Trim();
            if (await _context.Instrutores.AnyAsync(i => i.Cref == cref && i.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Cref), "Já existe um instrutor cadastrado com este CREF.");
            }
        }

        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            if (await _context.Instrutores.AnyAsync(i => i.Email == model.Email && i.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Email), "Já existe um instrutor cadastrado com este e-mail.");
            }
            else
            {
                // O e-mail não pode ser o login de outra pessoa (por exemplo, do administrador).
                var outro = await _userManager.FindByEmailAsync(model.Email);
                if (outro is not null && outro.Id != usuarioAtual?.Id)
                {
                    ModelState.AddModelError(nameof(model.Email), "Este e-mail já é usado como login por outro usuário.");
                }
            }
        }

        if (!string.IsNullOrEmpty(model.Senha))
        {
            var usuario = usuarioAtual ?? new IdentityUser { UserName = model.Email, Email = model.Email };
            foreach (var validador in _userManager.PasswordValidators)
            {
                var resultado = await validador.ValidateAsync(_userManager, usuario, model.Senha);
                foreach (var erro in resultado.Errors)
                {
                    ModelState.AddModelError(nameof(model.Senha), erro.Description);
                }
            }
        }
    }

    private async Task<IdentityUser?> CriarLoginAsync(string email, string senha)
    {
        var usuario = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

        var resultado = await _userManager.CreateAsync(usuario, senha);
        if (!AdicionarErros(resultado, nameof(InstrutorFormViewModel.Senha)))
        {
            return null;
        }

        resultado = await _userManager.AddToRoleAsync(usuario, Perfis.Instrutor);
        return AdicionarErros(resultado, string.Empty) ? usuario : null;
    }

    /// <summary>Bloqueia o login do instrutor inativo.</summary>
    private async Task DesativarLoginAsync(IdentityUser usuario)
    {
        await _userManager.SetLockoutEnabledAsync(usuario, true);
        await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
        await _userManager.UpdateSecurityStampAsync(usuario); // encerra sessões abertas
    }

    private bool AdicionarErros(IdentityResult resultado, string campo)
    {
        foreach (var erro in resultado.Errors)
        {
            ModelState.AddModelError(campo, erro.Description);
        }

        return resultado.Succeeded;
    }
}
