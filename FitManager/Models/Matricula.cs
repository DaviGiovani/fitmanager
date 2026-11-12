using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

public class Matricula
{
    public int Id { get; set; }

    [Display(Name = "Aluno")]
    public int AlunoId { get; set; }

    [Display(Name = "Plano")]
    public int PlanoId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de início")]
    public DateTime DataInicio { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de término")]
    public DateTime DataFim { get; set; }

    [Display(Name = "Valor mensal")]
    public decimal ValorMensal { get; set; }

    public StatusMatricula Status { get; set; } = StatusMatricula.Ativa;

    [DataType(DataType.Date)]
    [Display(Name = "Data de cancelamento")]
    public DateTime? DataCancelamento { get; set; }

    public Aluno? Aluno { get; set; }

    public Plano? Plano { get; set; }

    public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();

    /// <summary>RN03: data de término = data de início + duração do plano em meses.</summary>
    public void CalcularDataFim(int duracaoMeses)
    {
        DataFim = DataInicio.Date.AddMonths(duracaoMeses);
    }

    public void Cancelar()
    {
        if (Status != StatusMatricula.Ativa)
        {
            throw new InvalidOperationException("Somente matrículas ativas podem ser canceladas.");
        }

        Status = StatusMatricula.Cancelada;
        DataCancelamento = DateTime.Today;
    }

    public bool EstaAtiva() => Status == StatusMatricula.Ativa;

    /// <summary>Quantidade de mensalidades do período contratado.</summary>
    public int QuantidadeMensalidades()
    {
        var meses = (DataFim.Year - DataInicio.Year) * 12 + DataFim.Month - DataInicio.Month;
        return Math.Max(1, meses);
    }

    /// <summary>Meses de referência das mensalidades (sempre o dia 1º de cada mês).</summary>
    public IEnumerable<DateTime> MesesDeReferencia()
    {
        var primeiro = new DateTime(DataInicio.Year, DataInicio.Month, 1);
        var quantidade = QuantidadeMensalidades();

        for (var i = 0; i < quantidade; i++)
        {
            yield return primeiro.AddMonths(i);
        }
    }

    public Pagamento? PagamentoDoMes(DateTime mes)
    {
        return Pagamentos.FirstOrDefault(p => p.MesReferencia.Year == mes.Year && p.MesReferencia.Month == mes.Month);
    }

    /// <summary>Mensalidades do período que ainda não foram pagas (RF08).</summary>
    public IEnumerable<DateTime> MesesPendentes()
    {
        return MesesDeReferencia().Where(mes => PagamentoDoMes(mes) is null);
    }

    /// <summary>Mensalidades não pagas cujo mês de referência já começou.</summary>
    public IEnumerable<DateTime> MesesEmAberto(DateTime hoje)
    {
        var mesAtual = new DateTime(hoje.Year, hoje.Month, 1);
        return MesesPendentes().Where(mes => mes <= mesAtual);
    }
}
