using System.ComponentModel.DataAnnotations;

namespace FitManager.ViewModels;

public class TreinoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecione o aluno.")]
    [Display(Name = "Aluno")]
    public int? AlunoId { get; set; }

    [Required(ErrorMessage = "Selecione o instrutor.")]
    [Display(Name = "Instrutor")]
    public int? InstrutorId { get; set; }

    [Required(ErrorMessage = "Informe o nome da ficha.")]
    [StringLength(60, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
    [Display(Name = "Nome da ficha")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "O objetivo deve ter no máximo {1} caracteres.")]
    public string? Objetivo { get; set; }

    [Display(Name = "Ficha ativa")]
    public bool Ativo { get; set; } = true;

    public List<ItemTreinoViewModel> Itens { get; set; } = new();
}

public class ItemTreinoViewModel
{
    [Required(ErrorMessage = "Selecione o exercício.")]
    [Display(Name = "Exercício")]
    public int? ExercicioId { get; set; }

    [Required(ErrorMessage = "Informe as séries.")]
    [Range(1, 20, ErrorMessage = "As séries devem estar entre {1} e {2}.")]
    [Display(Name = "Séries")]
    public int? Series { get; set; } = 3;

    [Required(ErrorMessage = "Informe as repetições.")]
    [StringLength(20, ErrorMessage = "As repetições devem ter no máximo {1} caracteres.")]
    [Display(Name = "Repetições")]
    public string Repeticoes { get; set; } = "12";

    [Range(0, 999.99, ErrorMessage = "A carga deve estar entre {1} e {2} kg.")]
    [Display(Name = "Carga (kg)")]
    public decimal? CargaKg { get; set; }

    [Required(ErrorMessage = "Informe o descanso.")]
    [Range(0, 600, ErrorMessage = "O descanso deve estar entre {1} e {2} segundos.")]
    [Display(Name = "Descanso (s)")]
    public int? DescansoSegundos { get; set; } = 60;
}
