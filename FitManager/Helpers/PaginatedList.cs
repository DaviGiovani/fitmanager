using Microsoft.EntityFrameworkCore;

namespace FitManager.Helpers;

/// <summary>
/// Lista paginada: a contagem e o corte (Skip/Take) são executados no banco de dados (RNF07).
/// </summary>
public class PaginatedList<T> : List<T>
{
    public int PaginaAtual { get; }
    public int TotalPaginas { get; }
    public int TotalItens { get; }

    public bool TemAnterior => PaginaAtual > 1;
    public bool TemProxima => PaginaAtual < TotalPaginas;

    private PaginatedList(List<T> itens, int totalItens, int paginaAtual, int tamanhoPagina)
    {
        TotalItens = totalItens;
        PaginaAtual = paginaAtual;
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(totalItens / (double)tamanhoPagina));
        AddRange(itens);
    }

    public static async Task<PaginatedList<T>> CriarAsync(IQueryable<T> consulta, int pagina, int tamanhoPagina)
    {
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanhoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var itens = await consulta
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PaginatedList<T>(itens, total, pagina, tamanhoPagina);
    }
}
