namespace FitManager.Helpers;

public static class CpfUtil
{
    /// <summary>Remove pontos, traço e espaços, deixando apenas os números.</summary>
    public static string SomenteDigitos(string? valor)
    {
        return string.IsNullOrEmpty(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());
    }

    /// <summary>Formata 11 dígitos como 000.000.000-00.</summary>
    public static string Formatar(string? valor)
    {
        var d = SomenteDigitos(valor);
        return d.Length == 11
            ? $"{d[..3]}.{d.Substring(3, 3)}.{d.Substring(6, 3)}-{d.Substring(9, 2)}"
            : valor ?? string.Empty;
    }

    /// <summary>Confere os dois dígitos verificadores do CPF.</summary>
    public static bool EhValido(string? valor)
    {
        var cpf = SomenteDigitos(valor);

        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
        {
            return false;
        }

        var primeiro = CalcularDigito(cpf, 9);
        var segundo = CalcularDigito(cpf, 10);

        return cpf[9] - '0' == primeiro && cpf[10] - '0' == segundo;
    }

    private static int CalcularDigito(string cpf, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
        {
            soma += (cpf[i] - '0') * (quantidade + 1 - i);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
