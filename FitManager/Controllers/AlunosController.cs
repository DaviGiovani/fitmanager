using FitManager.Data;
using FitManager.Helpers;
using FitManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

/// <summary>UC01 – Manter Alunos (RF01, RF02).</summary>
[Authorize(Roles = Perfis.Administrador)]
public class AlunosController : Controller
{
    private const int TamanhoPagina = 10;
    private const string CamposDoFormulario = "Id,Nome,Cpf,DataNascimento,Telefone,Email,Ativo";

    private readonly AppDbContext _context;

    public AlunosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Alunos?busca=termo&pagina=1
    public async Task<IActionResult> Index(string? busca, int pagina = 1)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        var consulta = _context.Alunos
            .AsNoTracking()
            .Include(a => a.Matriculas.Where(m => m.Status == StatusMatricula.Ativa))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var digitos = CpfUtil.SomenteDigitos(termo);

            consulta = digitos.Length > 0
                ? consulta.Where(a => a.Nome.Contains(termo) || a.Cpf.Contains(digitos))
                : consulta.Where(a => a.Nome.Contains(termo));
        }

        var alunos = await PaginatedList<Aluno>.CriarAsync(consulta.OrderBy(a => a.Nome), pagina, TamanhoPagina);

        ViewData["Busca"] = busca;
        return View(alunos);
    }

    // GET: /Alunos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        await _context.EncerrarMatriculasVencidasAsync();

        var aluno = await _context.Alunos
            .AsNoTracking()
            .Include(a => a.Matriculas).ThenInclude(m => m.Plano)
            .FirstOrDefaultAsync(a => a.Id == id);

        return aluno is null ? NotFound() : View(aluno);
    }

    // GET: /Alunos/Create
    public IActionResult Create()
    {
        return View(new Aluno());
    }

    // POST: /Alunos/Create
    [HttpPost]
    public async Task<IActionResult> Create([Bind(CamposDoFormulario)] Aluno aluno)
    {
        aluno.Id = 0;
        await ValidarRegrasAsync(aluno);

        if (!ModelState.IsValid)
        {
            return View(aluno);
        }

        aluno.Nome = aluno.Nome.Trim();
        aluno.Cpf = CpfUtil.SomenteDigitos(aluno.Cpf);
        aluno.DataCadastro = DateTime.Now;

        _context.Alunos.Add(aluno);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = $"Aluno {aluno.Nome} cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Alunos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var aluno = await _context.Alunos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (aluno is null)
        {
            return NotFound();
        }

        aluno.Cpf = aluno.CpfFormatado;
        return View(aluno);
    }

    // POST: /Alunos/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind(CamposDoFormulario)] Aluno formulario)
    {
        if (id != formulario.Id)
        {
            return NotFound();
        }

        var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id);
        if (aluno is null)
        {
            return NotFound();
        }

        await ValidarRegrasAsync(formulario);

        if (!ModelState.IsValid)
        {
            return View(formulario);
        }

        aluno.Nome = formulario.Nome.Trim();
        aluno.Cpf = CpfUtil.SomenteDigitos(formulario.Cpf);
        aluno.DataNascimento = formulario.DataNascimento;
        aluno.Telefone = formulario.Telefone;
        aluno.Email = formulario.Email;
        aluno.Ativo = formulario.Ativo;

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = $"Dados de {aluno.Nome} atualizados com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Alunos/Delete/5  (tela de confirmação - RNF04)
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        await _context.EncerrarMatriculasVencidasAsync();

        var aluno = await _context.Alunos
            .AsNoTracking()
            .Include(a => a.Matriculas)
            .Include(a => a.Treinos)
            .FirstOrDefaultAsync(a => a.Id == id);

        return aluno is null ? NotFound() : View(aluno);
    }

    // POST: /Alunos/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id);
        if (aluno is null)
        {
            return NotFound();
        }

        // RN05: aluno com matrícula ativa não pode ser excluído.
        var possuiMatriculaAtiva = await _context.Matriculas
            .AnyAsync(m => m.AlunoId == id && m.Status == StatusMatricula.Ativa);

        if (possuiMatriculaAtiva)
        {
            TempData["Erro"] = $"Não é possível excluir {aluno.Nome}: o aluno possui matrícula ativa. Cancele a matrícula antes de excluir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _context.Alunos.Remove(aluno);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = $"Aluno {aluno.Nome} excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Validações que dependem de regra de negócio ou do banco de dados.</summary>
    private async Task ValidarRegrasAsync(Aluno aluno)
    {
        if (aluno.DataNascimento.Date > DateTime.Today)
        {
            ModelState.AddModelError(nameof(Aluno.DataNascimento), "A data de nascimento não pode ser uma data futura.");
        }
        else if (aluno.DataNascimento.Year < 1900)
        {
            ModelState.AddModelError(nameof(Aluno.DataNascimento), "Informe uma data de nascimento válida.");
        }

        // RN01: o CPF não pode se repetir entre alunos.
        var cpf = CpfUtil.SomenteDigitos(aluno.Cpf);
        if (CpfUtil.EhValido(cpf) && await _context.Alunos.AnyAsync(a => a.Cpf == cpf && a.Id != aluno.Id))
        {
            ModelState.AddModelError(nameof(Aluno.Cpf), "Já existe um aluno cadastrado com este CPF.");
        }
    }
}
