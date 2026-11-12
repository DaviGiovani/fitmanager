using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class Plano
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do plano.")]
    [StringLength(60, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "A descrição deve ter no máximo {1} caracteres.")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Informe a duração em meses.")]
    [Range(1, 60, ErrorMessage = "A duração deve ser de {1} a {2} meses.")]
    [Display(Name = "Duração (meses)")]
    public int DuracaoMeses { get; set; } = 1;

    [Required(ErrorMessage = "Informe o valor mensal.")]
    [Range(0.01, 99999.99, ErrorMessage = "O valor mensal deve ser maior que zero.")]
    [Display(Name = "Valor mensal")]
    public decimal ValorMensal { get; set; }

    public bool Ativo { get; set; } = true;

    /// <summary>Quando marcado, o GymPass aparece como forma de pagamento das matrículas deste plano.</summary>
    [Display(Name = "Aceita GymPass")]
    public bool AceitaGymPass { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    /// <summary>RN05: plano já utilizado em matrículas não é excluído, apenas inativado.</summary>
    public void Inativar() => Ativo = false;
}
