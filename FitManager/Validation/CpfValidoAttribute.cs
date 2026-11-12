using System.ComponentModel.DataAnnotations;
using FitManager.Helpers;

namespace FitManager.Validation;

/// <summary>
/// Valida o CPF pelos dígitos verificadores (RN01). Aceita o valor com ou sem máscara.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class CpfValidoAttribute : ValidationAttribute
{
    public CpfValidoAttribute()
    {
        ErrorMessage = "CPF inválido.";
    }

    public override bool IsValid(object? value)
    {
        // Campo vazio é tratado pelo [Required].
        if (value is not string texto || string.IsNullOrWhiteSpace(texto))
        {
            return true;
        }

        return CpfUtil.EhValido(texto);
    }
}
