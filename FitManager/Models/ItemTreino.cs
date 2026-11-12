using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

/// <summary>Exercício dentro de uma ficha de treino (tabela associativa ITEM_TREINO).</summary>
public class ItemTreino
{
    public int Id { get; set; }

    public int TreinoId { get; set; }

    public int ExercicioId { get; set; }

    public int Ordem { get; set; }

    [Display(Name = "Séries")]
    public int Series { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Repetições")]
    public string Repeticoes { get; set; } = string.Empty;

    [Display(Name = "Carga (kg)")]
    public decimal? CargaKg { get; set; }

    [Display(Name = "Descanso (s)")]
    public int DescansoSegundos { get; set; }

    public Treino? Treino { get; set; }

    public Exercicio? Exercicio { get; set; }
}
