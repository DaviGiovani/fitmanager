using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public enum StatusMatricula
{
    Ativa,
    Cancelada,
    Encerrada
}

public enum FormaPagamento
{
    Dinheiro,

    Pix,

    [Display(Name = "Cartão de débito")]
    CartaoDebito,

    [Display(Name = "Cartão de crédito")]
    CartaoCredito,

    Boleto,

    Cheque,

    /// <summary>Benefício corporativo; só é aceito em planos marcados como "Aceita GymPass".</summary>
    GymPass
}

public static class EnumExtensions
{
    public static string Descricao(this FormaPagamento forma) => forma switch
    {
        FormaPagamento.CartaoDebito => "Cartão de débito",
        FormaPagamento.CartaoCredito => "Cartão de crédito",
        _ => forma.ToString()
    };
}

public static class FormasPagamento
{
    /// <summary>
    /// Formas de pagamento aceitas para um plano: todas, exceto o GymPass
    /// quando o plano não aceita esse benefício.
    /// </summary>
    public static IEnumerable<FormaPagamento> Permitidas(Plano? plano)
    {
        return Enum.GetValues<FormaPagamento>()
            .Where(forma => forma != FormaPagamento.GymPass || plano?.AceitaGymPass == true);
    }
}
