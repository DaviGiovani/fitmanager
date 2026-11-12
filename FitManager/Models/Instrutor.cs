using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class Instrutor
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Display(Name = "CREF")]
    public string Cref { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(100)]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<Treino> Treinos { get; set; } = new List<Treino>();
}
