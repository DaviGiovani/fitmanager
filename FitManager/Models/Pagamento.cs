using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class Pagamento
{
    public int Id { get; set; }

    public int MatriculaId { get; set; }

    /// <summary>Sempre o dia 1º do mês a que a mensalidade se refere.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "Mês de referência")]
    public DateTime MesReferencia { get; set; }

    public decimal Valor { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data do pagamento")]
    public DateTime DataPagamento { get; set; }

    [Display(Name = "Forma de pagamento")]
    public FormaPagamento FormaPagamento { get; set; }

    public Matricula? Matricula { get; set; }
}
