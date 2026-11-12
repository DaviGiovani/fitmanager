using FitManager.Data;
using FitManager.Helpers;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

/// <summary>UC04 – Registrar Pagamento (RF07, RF08).</summary>
[Authorize(Roles = Perfis.Administrador)]
public class PagamentosController : Controller
{
    private const int TamanhoPagina = 15;

    private readonly AppDbContext _context;

    public PagamentosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Pagamentos  (histórico geral)
    public async Task<IActionResult> Index(string? busca, int pagina = 1)
    {
        var consulta = _context.Pagamentos
            .AsNoTracking()
            .Include(p => p.Matricula!).ThenInclude(m => m.Aluno)
            .Include(p => p.Matricula!).ThenInclude(m => m.Plano)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(p => p.Matricula!.Aluno!.Nome.Contains(termo));
        }

        var pagamentos = await PaginatedList<Pagamento>.CriarAsync(
            consulta.OrderByDescending(p => p.DataPagamento).ThenByDescending(p => p.Id),
            pagina,
            TamanhoPagina);

        ViewData["Busca"] = busca;
        return View(pagamentos);
    }

    // GET: /Pagamentos/Create?matriculaId=5
    public async Task<IActionResult> Create(int? matriculaId, DateTime? mes)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        // Sem matrícula informada: mostra a lista de matrículas ativas para escolher.
        if (matriculaId is null)
        {
            var ativas = await _context.Matriculas
                .AsNoTracking()
                .Include(m => m.Aluno)
                .Include(m => m.Plano)
                .Include(m => m.Pagamentos)
                .Where(m => m.Status == StatusMatricula.Ativa)
                .OrderBy(m => m.Aluno!.Nome)
                .ToListAsync();

            return View("Selecionar", ativas);
        }

        var matricula = await BuscarMatriculaAsync(matriculaId.Value);
        if (matricula is null)
        {
            return NotFound();
        }

        // RN06: só é possível registrar pagamento para matrícula ativa.
        if (!matricula.EstaAtiva())
        {
            TempData["Erro"] = "Não é possível registrar pagamento: a matrícula está cancelada ou encerrada.";
            return RedirectToAction("Details", "Matriculas", new { id = matricula.Id });
        }

        var pendentes = matricula.MesesPendentes().ToList();
        if (pendentes.Count == 0)
        {
            TempData["Aviso"] = "Todas as mensalidades desta matrícula já foram pagas.";
            return RedirectToAction("Details", "Matriculas", new { id = matricula.Id });
        }

        var mesSugerido = mes.HasValue && pendentes.Contains(new DateTime(mes.Value.Year, mes.Value.Month, 1))
            ? new DateTime(mes.Value.Year, mes.Value.Month, 1)
            : pendentes.First();

        ViewBag.Matricula = matricula;

        return View(new PagamentoCreateViewModel
        {
            MatriculaId = matricula.Id,
            MesReferencia = mesSugerido,
            Valor = matricula.ValorMensal,
            DataPagamento = DateTime.Today
        });
    }

    // POST: /Pagamentos/Create
    [HttpPost]
    public async Task<IActionResult> Create(PagamentoCreateViewModel model)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        var matricula = await BuscarMatriculaAsync(model.MatriculaId);
        if (matricula is null)
        {
            return NotFound();
        }

        // RN06: só é possível registrar pagamento para matrícula ativa.
        if (!matricula.EstaAtiva())
        {
            TempData["Erro"] = "Não é possível registrar pagamento: a matrícula está cancelada ou encerrada.";
            return RedirectToAction("Details", "Matriculas", new { id = matricula.Id });
        }

        if (ModelState.IsValid)
        {
            var mes = new DateTime(model.MesReferencia!.Value.Year, model.MesReferencia.Value.Month, 1);

            if (!matricula.MesesDeReferencia().Contains(mes))
            {
                ModelState.AddModelError(nameof(model.MesReferencia), "O mês de referência não pertence ao período desta matrícula.");
            }
            else if (matricula.PagamentoDoMes(mes) is not null)
            {
                // RN06: cada mês de referência pode ser pago uma única vez.
                ModelState.AddModelError(nameof(model.MesReferencia), $"A mensalidade de {mes:MM/yyyy} já está quitada.");
            }

            // GymPass só é aceito quando o plano da matrícula permite.
            if (!FormasPagamento.Permitidas(matricula.Plano).Contains(model.FormaPagamento!.Value))
            {
                ModelState.AddModelError(nameof(model.FormaPagamento), "O plano desta matrícula não aceita GymPass.");
            }

            if (model.DataPagamento!.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DataPagamento), "A data do pagamento não pode ser uma data futura.");
            }

            if (ModelState.IsValid)
            {
                var pagamento = new Pagamento
                {
                    MatriculaId = matricula.Id,
                    MesReferencia = mes,
                    Valor = model.Valor!.Value,
                    DataPagamento = model.DataPagamento.Value.Date,
                    FormaPagamento = model.FormaPagamento!.Value
                };

                _context.Pagamentos.Add(pagamento);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = $"Pagamento da mensalidade de {mes:MM/yyyy} registrado com sucesso.";
                return RedirectToAction("Details", "Matriculas", new { id = matricula.Id });
            }
        }

        ViewBag.Matricula = matricula;
        return View(model);
    }

    private Task<Matricula?> BuscarMatriculaAsync(int id)
    {
        return _context.Matriculas
            .AsNoTracking()
            .Include(m => m.Aluno)
            .Include(m => m.Plano)
            .Include(m => m.Pagamentos)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
