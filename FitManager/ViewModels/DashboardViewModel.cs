using FitManager.Models;

namespace FitManager.ViewModels;

public class DashboardViewModel
{
    // Administrador
    public int AlunosAtivos { get; set; }
    public int MatriculasAtivas { get; set; }
    public int PlanosAtivos { get; set; }
    public decimal ReceitaDoMes { get; set; }
    public List<MensalidadeEmAbertoViewModel> MensalidadesEmAberto { get; set; } = new();
    public List<Matricula> MatriculasAVencer { get; set; } = new();

    // Instrutor
    public int FichasAtivas { get; set; }
    public List<Aluno> AlunosSemFicha { get; set; } = new();
    public List<Treino> UltimasFichas { get; set; } = new();
}

public class MensalidadeEmAbertoViewModel
{
    public int MatriculaId { get; set; }
    public string Aluno { get; set; } = string.Empty;
    public string Plano { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public DateTime MesMaisAntigo { get; set; }
    public decimal ValorTotal { get; set; }
}
