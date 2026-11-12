using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Controllers;

/// <summary>UC05 – Montar Ficha de Treino (RF09, RF10).</summary>
[Authorize(Roles = Perfis.Instrutor)]
public class TreinosController : Controller
{
    private const string MensagemFichaDeOutro =
        "Você só pode alterar ou excluir as fichas de treino que você elaborou.";

    private const string MensagemSemInstrutor =
        "O seu usuário não está vinculado a um cadastro de instrutor ativo. Procure o administrador.";

    private readonly AppDbContext _context;

    public TreinosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Treinos?busca=nome
    public async Task<IActionResult> Index(string? busca)
    {
        var consulta = _context.Treinos
            .AsNoTracking()
            .Include(t => t.Aluno)
            .Include(t => t.Instrutor)
            .Include(t => t.Itens)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(t => t.Aluno!.Nome.Contains(termo) || t.Nome.Contains(termo));
        }

        var treinos = await consulta
            .OrderByDescending(t => t.Ativo)
            .ThenBy(t => t.Aluno!.Nome)
            .ThenByDescending(t => t.DataCriacao)
            .ToListAsync();

        ViewData["Busca"] = busca;
        ViewBag.InstrutorLogadoId = (await BuscarInstrutorLogadoAsync())?.Id;
        return View(treinos);
    }

    // GET: /Treinos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var treino = await _context.Treinos
            .AsNoTracking()
            .Include(t => t.Aluno)
            .Include(t => t.Instrutor)
            .Include(t => t.Itens).ThenInclude(i => i.Exercicio)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (treino is null)
        {
            return NotFound();
        }

        ViewBag.InstrutorLogadoId = (await BuscarInstrutorLogadoAsync())?.Id;
        return View(treino);
    }

    // GET: /Treinos/Create?alunoId=5
    public async Task<IActionResult> Create(int? alunoId)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        // RN07: a ficha só pode ser criada para aluno com matrícula ativa.
        if (alunoId.HasValue && !await AlunoPossuiMatriculaAtivaAsync(alunoId.Value))
        {
            TempData["Erro"] = "Este aluno não possui matrícula ativa. A ficha de treino só pode ser criada para alunos com matrícula ativa.";
            return RedirectToAction(nameof(Index));
        }

        // A ficha é registrada sempre no nome do instrutor logado.
        var instrutor = await BuscarInstrutorLogadoAsync();
        if (instrutor is null)
        {
            TempData["Erro"] = MensagemSemInstrutor;
            return RedirectToAction(nameof(Index));
        }

        var model = new TreinoFormViewModel
        {
            AlunoId = alunoId,
            InstrutorId = instrutor.Id,
            Itens = { new ItemTreinoViewModel() }
        };

        await CarregarListasAsync(model.AlunoId);
        return View(model);
    }

    // POST: /Treinos/Create
    [HttpPost]
    public async Task<IActionResult> Create(TreinoFormViewModel model)
    {
        await _context.EncerrarMatriculasVencidasAsync();

        // O instrutor da ficha é sempre o usuário logado, nunca o valor enviado pelo formulário.
        var instrutor = await BuscarInstrutorLogadoAsync();
        if (instrutor is null)
        {
            TempData["Erro"] = MensagemSemInstrutor;
            return RedirectToAction(nameof(Index));
        }

        model.InstrutorId = instrutor.Id;
        ModelState.Remove(nameof(model.InstrutorId));

        await ValidarAsync(model, novaFicha: true);

        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(model.AlunoId);
            return View(model);
        }

        var treino = new Treino
        {
            AlunoId = model.AlunoId!.Value,
            InstrutorId = model.InstrutorId!.Value,
            Nome = model.Nome.Trim(),
            Objetivo = model.Objetivo,
            DataCriacao = DateTime.Today,
            Ativo = model.Ativo
        };

        foreach (var item in model.Itens)
        {
            treino.AdicionarItem(ConverterItem(item));
        }

        // A ficha e os itens são gravados na mesma transação (um único SaveChanges).
        _context.Treinos.Add(treino);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Ficha de treino criada com sucesso.";
        return RedirectToAction(nameof(Details), new { id = treino.Id });
    }

    // GET: /Treinos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var treino = await _context.Treinos
            .AsNoTracking()
            .Include(t => t.Itens)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (treino is null)
        {
            return NotFound();
        }

        if (!await PodeAlterarAsync(treino))
        {
            TempData["Erro"] = MensagemFichaDeOutro;
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new TreinoFormViewModel
        {
            Id = treino.Id,
            AlunoId = treino.AlunoId,
            InstrutorId = treino.InstrutorId,
            Nome = treino.Nome,
            Objetivo = treino.Objetivo,
            Ativo = treino.Ativo,
            Itens = treino.Itens
                .OrderBy(i => i.Ordem)
                .Select(i => new ItemTreinoViewModel
                {
                    ExercicioId = i.ExercicioId,
                    Series = i.Series,
                    Repeticoes = i.Repeticoes,
                    CargaKg = i.CargaKg,
                    DescansoSegundos = i.DescansoSegundos
                })
                .ToList()
        };

        await CarregarListasAsync(model.AlunoId);
        return View(model);
    }

    // POST: /Treinos/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, TreinoFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var treino = await _context.Treinos
            .Include(t => t.Itens)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (treino is null)
        {
            return NotFound();
        }

        if (!await PodeAlterarAsync(treino))
        {
            TempData["Erro"] = MensagemFichaDeOutro;
            return RedirectToAction(nameof(Details), new { id });
        }

        // O aluno e o instrutor da ficha não mudam na edição.
        model.AlunoId = treino.AlunoId;
        model.InstrutorId = treino.InstrutorId;
        ModelState.Remove(nameof(model.AlunoId));
        ModelState.Remove(nameof(model.InstrutorId));

        await ValidarAsync(model, novaFicha: false);

        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(model.AlunoId);
            return View(model);
        }

        treino.Nome = model.Nome.Trim();
        treino.Objetivo = model.Objetivo;
        treino.Ativo = model.Ativo;

        // Substitui os itens antigos pelos itens enviados no formulário.
        _context.ItensTreino.RemoveRange(treino.Itens);
        treino.Itens.Clear();

        foreach (var item in model.Itens)
        {
            treino.AdicionarItem(ConverterItem(item));
        }

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Ficha de treino atualizada com sucesso.";
        return RedirectToAction(nameof(Details), new { id = treino.Id });
    }

    // GET: /Treinos/Delete/5  (tela de confirmação - RNF04)
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var treino = await _context.Treinos
            .AsNoTracking()
            .Include(t => t.Aluno)
            .Include(t => t.Instrutor)
            .Include(t => t.Itens)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (treino is null)
        {
            return NotFound();
        }

        if (!await PodeAlterarAsync(treino))
        {
            TempData["Erro"] = MensagemFichaDeOutro;
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(treino);
    }

    // POST: /Treinos/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var treino = await _context.Treinos.FirstOrDefaultAsync(t => t.Id == id);
        if (treino is null)
        {
            return NotFound();
        }

        if (!await PodeAlterarAsync(treino))
        {
            TempData["Erro"] = MensagemFichaDeOutro;
            return RedirectToAction(nameof(Details), new { id });
        }

        _context.Treinos.Remove(treino); // os itens são removidos em cascata
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Ficha de treino excluída com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Métodos auxiliares ----------

    private static ItemTreino ConverterItem(ItemTreinoViewModel item)
    {
        return new ItemTreino
        {
            ExercicioId = item.ExercicioId!.Value,
            Series = item.Series!.Value,
            Repeticoes = item.Repeticoes.Trim(),
            CargaKg = item.CargaKg,
            DescansoSegundos = item.DescansoSegundos!.Value
        };
    }

    private async Task ValidarAsync(TreinoFormViewModel model, bool novaFicha)
    {
        // RN07: a ficha deve conter ao menos um exercício.
        if (model.Itens.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "A ficha de treino deve conter ao menos um exercício.");
        }

        if (model.AlunoId.HasValue)
        {
            if (!await _context.Alunos.AnyAsync(a => a.Id == model.AlunoId))
            {
                ModelState.AddModelError(nameof(model.AlunoId), "Aluno não encontrado.");
            }
            else if (novaFicha && !await AlunoPossuiMatriculaAtivaAsync(model.AlunoId.Value))
            {
                // RN07: ficha só para aluno com matrícula ativa.
                ModelState.AddModelError(nameof(model.AlunoId), "O aluno selecionado não possui matrícula ativa.");
            }
        }

        if (model.InstrutorId.HasValue && !await _context.Instrutores.AnyAsync(i => i.Id == model.InstrutorId))
        {
            ModelState.AddModelError(nameof(model.InstrutorId), "Instrutor não encontrado.");
        }

        var exercicios = model.Itens
            .Where(i => i.ExercicioId.HasValue)
            .Select(i => i.ExercicioId!.Value)
            .ToList();

        if (exercicios.Count > 0)
        {
            var distintos = exercicios.Distinct().ToList();
            var existentes = await _context.Exercicios.CountAsync(e => distintos.Contains(e.Id));

            if (existentes != distintos.Count)
            {
                ModelState.AddModelError(string.Empty, "Um dos exercícios selecionados não existe mais.");
            }

            if (distintos.Count != exercicios.Count)
            {
                ModelState.AddModelError(string.Empty, "O mesmo exercício foi adicionado mais de uma vez na ficha.");
            }
        }
    }

    private Task<bool> AlunoPossuiMatriculaAtivaAsync(int alunoId)
    {
        return _context.Matriculas.AnyAsync(m => m.AlunoId == alunoId && m.Status == StatusMatricula.Ativa);
    }

    /// <summary>Procura o instrutor cujo e-mail é o mesmo do usuário logado.</summary>
    private Task<Instrutor?> BuscarInstrutorLogadoAsync()
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult<Instrutor?>(null);
        }

        return _context.Instrutores
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Email == email && i.Ativo);
    }

    /// <summary>RN10: um instrutor só altera e exclui as fichas que ele mesmo elaborou.</summary>
    private async Task<bool> PodeAlterarAsync(Treino treino)
    {
        var instrutor = await BuscarInstrutorLogadoAsync();
        return instrutor is not null && instrutor.Id == treino.InstrutorId;
    }

    private async Task CarregarListasAsync(int? alunoSelecionadoId)
    {
        // Alunos com matrícula ativa (mais o aluno da ficha, no caso de edição).
        ViewBag.Alunos = await _context.Alunos
            .AsNoTracking()
            .Where(a => a.Matriculas.Any(m => m.Status == StatusMatricula.Ativa) || a.Id == alunoSelecionadoId)
            .OrderBy(a => a.Nome)
            .ToListAsync();

        ViewBag.Instrutores = await _context.Instrutores
            .AsNoTracking()
            .Where(i => i.Ativo)
            .OrderBy(i => i.Nome)
            .ToListAsync();

        ViewBag.Exercicios = await _context.Exercicios
            .AsNoTracking()
            .OrderBy(e => e.GrupoMuscular)
            .ThenBy(e => e.Nome)
            .ToListAsync();
    }
}
