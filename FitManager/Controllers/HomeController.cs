using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        await _context.EncerrarMatriculasVencidasAsync();

        var hoje = DateTime.Today;
        var model = new DashboardViewModel();

        if (User.IsInRole(Perfis.Administrador))
        {
            var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);
            var inicioProximoMes = inicioDoMes.AddMonths(1);

            model.AlunosAtivos = await _context.Alunos.CountAsync(a => a.Ativo);
            model.PlanosAtivos = await _context.Planos.CountAsync(p => p.Ativo);
            model.ReceitaDoMes = await _context.Pagamentos
                .Where(p => p.DataPagamento >= inicioDoMes && p.DataPagamento < inicioProximoMes)
                .SumAsync(p => (decimal?)p.Valor) ?? 0m;

            var ativas = await _context.Matriculas
                .AsNoTracking()
                .Include(m => m.Aluno)
                .Include(m => m.Plano)
                .Include(m => m.Pagamentos)
                .Where(m => m.Status == StatusMatricula.Ativa)
                .ToListAsync();

            model.MatriculasAtivas = ativas.Count;

            model.MensalidadesEmAberto = ativas
                .Select(m => new { Matricula = m, EmAberto = m.MesesEmAberto(hoje).ToList() })
                .Where(x => x.EmAberto.Count > 0)
                .Select(x => new MensalidadeEmAbertoViewModel
                {
                    MatriculaId = x.Matricula.Id,
                    Aluno = x.Matricula.Aluno?.Nome ?? string.Empty,
                    Plano = x.Matricula.Plano?.Nome ?? string.Empty,
                    Quantidade = x.EmAberto.Count,
                    MesMaisAntigo = x.EmAberto.Min(),
                    ValorTotal = x.EmAberto.Count * x.Matricula.ValorMensal
                })
                .OrderBy(x => x.MesMaisAntigo)
                .ThenBy(x => x.Aluno)
                .ToList();

            model.MatriculasAVencer = ativas
                .Where(m => m.DataFim <= hoje.AddDays(30))
                .OrderBy(m => m.DataFim)
                .ToList();
        }

        if (User.IsInRole(Perfis.Instrutor))
        {
            model.FichasAtivas = await _context.Treinos.CountAsync(t => t.Ativo);

            model.AlunosSemFicha = await _context.Alunos
                .AsNoTracking()
                .Where(a => a.Matriculas.Any(m => m.Status == StatusMatricula.Ativa)
                            && !a.Treinos.Any(t => t.Ativo))
                .OrderBy(a => a.Nome)
                .ToListAsync();

            model.UltimasFichas = await _context.Treinos
                .AsNoTracking()
                .Include(t => t.Aluno)
                .Include(t => t.Instrutor)
                .OrderByDescending(t => t.DataCriacao)
                .ThenByDescending(t => t.Id)
                .Take(5)
                .ToListAsync();
        }

        return View(model);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Erro()
    {
        return View();
    }
}
