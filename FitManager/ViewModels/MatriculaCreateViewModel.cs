using System.ComponentModel.DataAnnotations;

namespace FitManager.ViewModels;

public class MatriculaCreateViewModel
{
    [Required(ErrorMessage = "Selecione o aluno.")]
    [Display(Name = "Aluno")]
    public int? AlunoId { get; set; }

    [Required(ErrorMessage = "Selecione o plano.")]
    [Display(Name = "Plano")]
    public int? PlanoId { get; set; }

    [Required(ErrorMessage = "Informe a data de início.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de início")]
    public DateTime? DataInicio { get; set; } = DateTime.Today;
}
