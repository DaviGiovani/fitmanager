using System.ComponentModel.DataAnnotations;

namespace FitManager.ViewModels;

public class MinhaContaViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo {1} caracteres.")]
    [Display(Name = "E-mail de acesso")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string? NovaSenha { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NovaSenha), ErrorMessage = "A confirmação não confere com a nova senha.")]
    [Display(Name = "Confirmar nova senha")]
    public string? ConfirmarNovaSenha { get; set; }

    [Required(ErrorMessage = "Informe a sua senha atual para confirmar as alterações.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string SenhaAtual { get; set; } = string.Empty;
}
