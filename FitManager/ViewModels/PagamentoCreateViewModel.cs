using System.ComponentModel.DataAnnotations;
using FitManager.Models;

namespace FitManager.ViewModels;

public class PagamentoCreateViewModel
{
    public int MatriculaId { get; set; }

    [Required(ErrorMessage = "Selecione o mês de referência.")]
    [Display(Name = "Mês de referência")]
    public DateTime? MesReferencia { get; set; }

    [Required(ErrorMessage = "Informe o valor.")]
    [Range(0.01, 99999.99, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal? Valor { get; set; }

    [Required(ErrorMessage = "Informe a data do pagamento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data do pagamento")]
    public DateTime? DataPagamento { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Selecione a forma de pagamento.")]
    [Display(Name = "Forma de pagamento")]
    public FormaPagamento? FormaPagamento { get; set; }
}
