using FitManager.Data;
using FitManager.Helpers;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

/// <summary>UC03 – Realizar Matrícula (RF05, RF06).</summary>
[Authorize(Roles = Perfis.Administrador)]
public class MatriculasController : Controller
{
    private const int TamanhoPagina = 10;

    private readonly AppDbContext _context;

    public MatriculasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Matriculas?status=Ativa&busca=nome
    public async Task<IActionResult> Index(StatusMatricula? status, string? busca, int pagina = 1)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        var consulta = _context.Matriculas
            .AsNoTracking()
            .Include(m => m.Aluno)
            .Include(m => m.Plano)
            .AsQueryable();

        if (status.HasValue)
        {
            var filtro = status.Value;
            consulta = consulta.Where(m => m.Status == filtro);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(m => m.Aluno!.Nome.Contains(termo));
        }

        var matriculas = await PaginatedList<Matricula>.CriarAsync(
            consulta.OrderByDescending(m => m.DataInicio).ThenByDescending(m => m.Id),
            pagina,
            TamanhoPagina);

        ViewData["Status"] = status;
        ViewData["Busca"] = busca;
        return View(matriculas);
    }

    // GET: /Matriculas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        await _context.EncerrarMatriculasVencidasAsync();

        var matricula = await _context.Matriculas
            .AsNoTracking()
            .Include(m => m.Aluno)
            .Include(m => m.Plano)
            .Include(m => m.Pagamentos)
            .FirstOrDefaultAsync(m => m.Id == id);

        return matricula is null ? NotFound() : View(matricula);
    }

    // GET: /Matriculas/Create?alunoId=5
    public async Task<IActionResult> Create(int? alunoId)
    {
        await CarregarListasAsync();
        return View(new MatriculaCreateViewModel { AlunoId = alunoId });
    }

    // POST: /Matriculas/Create
    [HttpPost]
    public async Task<IActionResult> Create(MatriculaCreateViewModel model)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        Aluno? aluno = null;
        Plano? plano = null;

        if (ModelState.IsValid)
        {
            aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == model.AlunoId && a.Ativo);
            if (aluno is null)
            {
                ModelState.AddModelError(nameof(model.AlunoId), "Aluno não encontrado ou inativo.");
            }

            plano = await _context.Planos.FirstOrDefaultAsync(p => p.Id == model.PlanoId && p.Ativo);
            if (plano is null)
            {
                ModelState.AddModelError(nameof(model.PlanoId), "Plano não encontrado ou inativo.");
            }
        }

        // RN02: um aluno pode ter apenas uma matrícula ativa por vez.
        if (aluno is not null)
        {
            var alunoId = aluno.Id;
            var possuiMatriculaAtiva = await _context.Matriculas
                .AnyAsync(m => m.AlunoId == alunoId && m.Status == StatusMatricula.Ativa);

            if (possuiMatriculaAtiva)
            {
                ModelState.AddModelError(string.Empty, $"{aluno.Nome} já possui uma matrícula ativa. Cancele a matrícula atual antes de criar outra.");
            }
        }

        if (ModelState.IsValid && aluno is not null && plano is not null)
        {
            var matricula = new Matricula
            {
                AlunoId = aluno.Id,
                PlanoId = plano.Id,
                DataInicio = model.DataInicio!.Value.Date,
                ValorMensal = plano.ValorMensal, // RN04: valor copiado do plano no momento da matrícula
                Status = StatusMatricula.Ativa
            };

            matricula.CalcularDataFim(plano.DuracaoMeses); // RN03

            if (matricula.DataFim < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DataInicio), "Com esta data de início o período do plano já estaria encerrado.");
            }
            else
            {
                _context.Matriculas.Add(matricula);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = $"Matrícula de {aluno.Nome} no plano {plano.Nome} realizada com sucesso.";
                TempData["NovaMatricula"] = true;
                return RedirectToAction(nameof(Details), new { id = matricula.Id });
            }
        }

        await CarregarListasAsync();
        return View(model);
    }

    // POST: /Matriculas/Cancelar/5
    [HttpPost]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        var matricula = await _context.Matriculas.FirstOrDefaultAsync(m => m.Id == id);
        if (matricula is null)
        {
            return NotFound();
        }

        if (!matricula.EstaAtiva())
        {
            TempData["Erro"] = "Somente matrículas ativas podem ser canceladas.";
        }
        else
        {
            matricula.Cancelar();
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Matrícula cancelada com sucesso.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task CarregarListasAsync()
    {
        ViewBag.Alunos = await _context.Alunos
            .AsNoTracking()
            .Where(a => a.Ativo)
            .OrderBy(a => a.Nome)
            .ToListAsync();

        ViewBag.Planos = await _context.Planos
            .AsNoTracking()
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }
}
