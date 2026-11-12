using FitManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Data;

/// <summary>
/// Dados iniciais gravados pela migration (seed): exercícios e dois instrutores de exemplo.
/// Os exercícios não têm tela própria; os demais instrutores são cadastrados na tela Instrutores.
/// </summary>
public static class DadosIniciais
{
    public const string EmailInstrutorPadrao = "instrutor@fitmanager.com";

    public static void Configurar(ModelBuilder builder)
    {
        builder.Entity<Instrutor>().HasData(
            new Instrutor { Id = 1, Nome = "Carlos Henrique Souza", Cref = "012345-G/PR", Telefone = "(43) 99999-0001", Email = EmailInstrutorPadrao, Ativo = true },
            new Instrutor { Id = 2, Nome = "Mariana Alves Costa", Cref = "067890-G/PR", Telefone = "(43) 99999-0002", Email = "mariana@fitmanager.com", Ativo = true });

        builder.Entity<Exercicio>().HasData(
            // Peito
            new Exercicio { Id = 1, Nome = "Supino reto com barra", GrupoMuscular = "Peito", Descricao = "Deitado no banco, descer a barra até o peito e empurrar." },
            new Exercicio { Id = 2, Nome = "Supino inclinado com halteres", GrupoMuscular = "Peito", Descricao = "Banco inclinado a 30-45 graus." },
            new Exercicio { Id = 3, Nome = "Crucifixo na máquina (peck deck)", GrupoMuscular = "Peito", Descricao = "Fechar os braços à frente do corpo." },
            new Exercicio { Id = 4, Nome = "Flexão de braço", GrupoMuscular = "Peito", Descricao = "Com o peso do corpo." },
            // Costas
            new Exercicio { Id = 5, Nome = "Puxada frontal", GrupoMuscular = "Costas", Descricao = "Puxar a barra até a altura do peito." },
            new Exercicio { Id = 6, Nome = "Remada baixa", GrupoMuscular = "Costas", Descricao = "Puxar o triângulo em direção ao abdômen." },
            new Exercicio { Id = 7, Nome = "Remada curvada com barra", GrupoMuscular = "Costas", Descricao = "Tronco inclinado e coluna reta." },
            new Exercicio { Id = 8, Nome = "Barra fixa", GrupoMuscular = "Costas", Descricao = "Com o peso do corpo ou no gravitron." },
            // Pernas
            new Exercicio { Id = 9, Nome = "Agachamento livre", GrupoMuscular = "Pernas", Descricao = "Barra apoiada nos ombros." },
            new Exercicio { Id = 10, Nome = "Leg press 45°", GrupoMuscular = "Pernas", Descricao = "Empurrar a plataforma sem travar os joelhos." },
            new Exercicio { Id = 11, Nome = "Cadeira extensora", GrupoMuscular = "Pernas", Descricao = "Isola o quadríceps." },
            new Exercicio { Id = 12, Nome = "Mesa flexora", GrupoMuscular = "Pernas", Descricao = "Isola os posteriores de coxa." },
            new Exercicio { Id = 13, Nome = "Panturrilha em pé", GrupoMuscular = "Pernas", Descricao = "Elevação dos calcanhares." },
            new Exercicio { Id = 14, Nome = "Stiff", GrupoMuscular = "Pernas", Descricao = "Posteriores de coxa e glúteos." },
            // Ombros
            new Exercicio { Id = 15, Nome = "Desenvolvimento com halteres", GrupoMuscular = "Ombros", Descricao = "Empurrar os halteres acima da cabeça." },
            new Exercicio { Id = 16, Nome = "Elevação lateral", GrupoMuscular = "Ombros", Descricao = "Elevar os halteres até a linha dos ombros." },
            new Exercicio { Id = 17, Nome = "Elevação frontal", GrupoMuscular = "Ombros", Descricao = "Elevar os halteres à frente do corpo." },
            // Bíceps
            new Exercicio { Id = 18, Nome = "Rosca direta com barra", GrupoMuscular = "Bíceps", Descricao = "Cotovelos fixos ao lado do corpo." },
            new Exercicio { Id = 19, Nome = "Rosca alternada com halteres", GrupoMuscular = "Bíceps", Descricao = "Um braço de cada vez." },
            new Exercicio { Id = 20, Nome = "Rosca martelo", GrupoMuscular = "Bíceps", Descricao = "Pegada neutra." },
            // Tríceps
            new Exercicio { Id = 21, Nome = "Tríceps na polia (pulley)", GrupoMuscular = "Tríceps", Descricao = "Estender os cotovelos empurrando a barra para baixo." },
            new Exercicio { Id = 22, Nome = "Tríceps testa", GrupoMuscular = "Tríceps", Descricao = "Deitado, descer a barra em direção à testa." },
            new Exercicio { Id = 23, Nome = "Tríceps francês", GrupoMuscular = "Tríceps", Descricao = "Halter atrás da cabeça." },
            // Abdômen
            new Exercicio { Id = 24, Nome = "Abdominal supra", GrupoMuscular = "Abdômen", Descricao = "Elevar o tronco contraindo o abdômen." },
            new Exercicio { Id = 25, Nome = "Prancha isométrica", GrupoMuscular = "Abdômen", Descricao = "Manter a posição pelo tempo indicado nas repetições." },
            new Exercicio { Id = 26, Nome = "Elevação de pernas", GrupoMuscular = "Abdômen", Descricao = "Deitado ou na barra." },
            // Cardio
            new Exercicio { Id = 27, Nome = "Esteira", GrupoMuscular = "Cardio", Descricao = "Caminhada ou corrida; informar o tempo nas repetições." },
            new Exercicio { Id = 28, Nome = "Bicicleta ergométrica", GrupoMuscular = "Cardio", Descricao = "Informar o tempo nas repetições." });
    }
}
