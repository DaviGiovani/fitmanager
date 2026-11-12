using System.ComponentModel.DataAnnotations;

namespace FitManager.ViewModels;

public class InstrutorFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do instrutor.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CREF.")]
    [StringLength(20, ErrorMessage = "O CREF deve ter no máximo {1} caracteres.")]
    [Display(Name = "CREF")]
    public string Cref { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo {1} caracteres.")]
    public string? Telefone { get; set; }

    [Required(ErrorMessage = "Informe o e-mail. Ele será o login do instrutor.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo {1} caracteres.")]
    [Display(Name = "E-mail (login)")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Instrutor ativo")]
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Cadastro: senha inicial do login (obrigatória).
    /// Edição: redefine a senha, ou cria o login de um instrutor que ainda não tem acesso.
    /// </summary>
    [DataType(DataType.Password)]
    [Display(Name = "Senha de acesso")]
    public string? Senha { get; set; }

    /// <summary>Somente leitura: indica se o instrutor já tem login no sistema.</summary>
    public bool PossuiAcesso { get; set; }
}
