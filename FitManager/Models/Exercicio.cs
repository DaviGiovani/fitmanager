using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class Exercicio
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    [Display(Name = "Grupo muscular")]
    public string GrupoMuscular { get; set; } = string.Empty;

    [StringLength(255)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    public ICollection<ItemTreino> Itens { get; set; } = new List<ItemTreino>();
}
