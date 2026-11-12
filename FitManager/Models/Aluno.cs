using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FitManager.Helpers;
using FitManager.Validation;

namespace FitManager.Models;

public class Aluno
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do aluno.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    [CpfValido]
    [Display(Name = "CPF")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime DataNascimento { get; set; }

    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo {1} caracteres.")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo {1} caracteres.")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    public bool Ativo { get; set; } = true;

    [Display(Name = "Data de cadastro")]
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    public ICollection<Treino> Treinos { get; set; } = new List<Treino>();

    [NotMapped]
    public string CpfFormatado => CpfUtil.Formatar(Cpf);

    public int CalcularIdade()
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - DataNascimento.Year;
        if (DataNascimento.Date > hoje.AddYears(-idade))
        {
            idade--;
        }

        return idade;
    }

    /// <summary>RN02: usado para impedir duas matrículas ativas e a exclusão do aluno (RN05).</summary>
    public bool PossuiMatriculaAtiva() => Matriculas.Any(m => m.EstaAtiva());
}
