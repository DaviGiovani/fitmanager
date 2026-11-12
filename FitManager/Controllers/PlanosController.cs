using FitManager.Data;
using FitManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

/// <summary>UC02 – Manter Planos (RF03, RF04).</summary>
[Authorize(Roles = Perfis.Administrador)]
public class PlanosController : Controller
{
    private const string CamposDoFormulario = "Id,Nome,Descricao,DuracaoMeses,ValorMensal,Ativo,AceitaGymPass";

    private readonly AppDbContext _context;

    public PlanosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Planos
    public async Task<IActionResult> Index()
    {
        var planos = await _context.Planos
            .AsNoTracking()
            .OrderByDescending(p => p.Ativo)
            .ThenBy(p => p.Nome)
            .ToListAsync();

        // Quantidade de matrículas ativas por plano, calculada no banco.
        ViewBag.MatriculasAtivas = await _context.Matriculas
            .Where(m => m.Status == StatusMatricula.Ativa)
            .GroupBy(m => m.PlanoId)
            .Select(g => new { PlanoId = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.PlanoId, x => x.Quantidade);

        return View(planos);
    }

    // GET: /Planos/Create
    public IActionResult Create()
    {
        return View(new Plano());
    }

    // POST: /Planos/Create
    [HttpPost]
    public async Task<IActionResult> Create([Bind(CamposDoFormulario)] Plano plano)
    {
        if (!ModelState.IsValid)
        {
            return View(plano);
        }

        plano.Id = 0;
        plano.Nome = plano.Nome.Trim();

        _context.Planos.Add(plano);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = $"Plano {plano.Nome} cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Planos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var plano = await _context.Planos.FindAsync(id);
        return plano is null ? NotFound() : View(plano);
    }

    // POST: /Planos/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind(CamposDoFormulario)] Plano formulario)
    {
        if (id != formulario.Id)
        {
            return NotFound();
        }

        var plano = await _context.Planos.FirstOrDefaultAsync(p => p.Id == id);
        if (plano is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(formulario);
        }

        // RN04: o novo valor vale apenas para as próximas matrículas;
        // as matrículas existentes guardam o valor contratado.
        plano.Nome = formulario.Nome.Trim();
        plano.Descricao = formulario.Descricao;
        plano.DuracaoMeses = formulario.DuracaoMeses;
        plano.ValorMensal = formulario.ValorMensal;
        plano.Ativo = formulario.Ativo;
        plano.AceitaGymPass = formulario.AceitaGymPass;

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = $"Plano {plano.Nome} atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Planos/Delete/5  (tela de confirmação - RNF04)
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var plano = await _context.Planos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (plano is null)
        {
            return NotFound();
        }

        ViewBag.QuantidadeMatriculas = await _context.Matriculas.CountAsync(m => m.PlanoId == id);
        return View(plano);
    }

    // POST: /Planos/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var plano = await _context.Planos.FirstOrDefaultAsync(p => p.Id == id);
        if (plano is null)
        {
            return NotFound();
        }

        // RN05 / RF04: plano já utilizado em matrículas é inativado, preservando o histórico.
        var utilizado = await _context.Matriculas.AnyAsync(m => m.PlanoId == id);

        if (utilizado)
        {
            plano.Inativar();
            await _context.SaveChangesAsync();

            TempData["Aviso"] = $"O plano {plano.Nome} já foi utilizado em matrículas e por isso foi inativado, preservando o histórico.";
        }
        else
        {
            _context.Planos.Remove(plano);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Plano {plano.Nome} excluído com sucesso.";
        }

        return RedirectToAction(nameof(Index));
    }
}
