using FitManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Data;

/// <summary>
/// Contexto do Entity Framework Core. Herda de IdentityDbContext para incluir
/// as tabelas de usuários e perfis do ASP.NET Core Identity.
/// Os nomes das tabelas seguem o modelo de dados do sistema (ALUNO, PLANO, MATRICULA...).
/// </summary>
public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Plano> Planos => Set<Plano>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<Instrutor> Instrutores => Set<Instrutor>();
    public DbSet<Exercicio> Exercicios => Set<Exercicio>();
    public DbSet<Treino> Treinos => Set<Treino>();
    public DbSet<ItemTreino> ItensTreino => Set<ItemTreino>();

    /// <summary>
    /// Matrículas ativas cujo período já terminou passam para o status Encerrada.
    /// </summary>
    public async Task EncerrarMatriculasVencidasAsync()
    {
        var hoje = DateTime.Today;

        var vencidas = await Matriculas
            .Where(m => m.Status == StatusMatricula.Ativa && m.DataFim < hoje)
            .ToListAsync();

        if (vencidas.Count == 0)
        {
            return;
        }

        foreach (var matricula in vencidas)
        {
            matricula.Status = StatusMatricula.Encerrada;
        }

        await SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---------- ALUNO ----------
        builder.Entity<Aluno>(e =>
        {
            e.ToTable("ALUNO");
            e.Property(a => a.Nome).HasMaxLength(100).IsRequired();
            e.Property(a => a.Cpf).HasColumnType("char(11)").IsRequired();
            e.Property(a => a.DataNascimento).HasColumnType("date");
            e.Property(a => a.Telefone).HasMaxLength(20);
            e.Property(a => a.Email).HasMaxLength(100);
            e.Property(a => a.DataCadastro).HasColumnType("datetime2");
            e.HasIndex(a => a.Cpf).IsUnique(); // RN01
        });

        // ---------- PLANO ----------
        builder.Entity<Plano>(e =>
        {
            e.ToTable("PLANO");
            e.Property(p => p.Nome).HasMaxLength(60).IsRequired();
            e.Property(p => p.Descricao).HasMaxLength(255);
            e.Property(p => p.ValorMensal).HasColumnType("decimal(10,2)");
        });

        // ---------- MATRICULA ----------
        builder.Entity<Matricula>(e =>
        {
            e.ToTable("MATRICULA");
            e.Property(m => m.DataInicio).HasColumnType("date");
            e.Property(m => m.DataFim).HasColumnType("date");
            e.Property(m => m.DataCancelamento).HasColumnType("date");
            e.Property(m => m.ValorMensal).HasColumnType("decimal(10,2)");
            e.Property(m => m.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            // Excluir o aluno remove seu histórico de matrículas (a exclusão só é
            // permitida pelo sistema quando não há matrícula ativa - RN05).
            e.HasOne(m => m.Aluno)
                .WithMany(a => a.Matriculas)
                .HasForeignKey(m => m.AlunoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Plano com matrículas não pode ser excluído, apenas inativado (RN05).
            e.HasOne(m => m.Plano)
                .WithMany(p => p.Matriculas)
                .HasForeignKey(m => m.PlanoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- PAGAMENTO ----------
        builder.Entity<Pagamento>(e =>
        {
            e.ToTable("PAGAMENTO");
            e.Property(p => p.MesReferencia).HasColumnType("date");
            e.Property(p => p.DataPagamento).HasColumnType("date");
            e.Property(p => p.Valor).HasColumnType("decimal(10,2)");
            e.Property(p => p.FormaPagamento).HasConversion<string>().HasMaxLength(20).IsRequired();

            // RN06: cada mês de referência só pode ser pago uma vez por matrícula.
            e.HasIndex(p => new { p.MatriculaId, p.MesReferencia }).IsUnique();

            // Composição: o pagamento não existe sem a matrícula.
            e.HasOne(p => p.Matricula)
                .WithMany(m => m.Pagamentos)
                .HasForeignKey(p => p.MatriculaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- INSTRUTOR ----------
        builder.Entity<Instrutor>(e =>
        {
            e.ToTable("INSTRUTOR");
            e.Property(i => i.Nome).HasMaxLength(100).IsRequired();
            e.Property(i => i.Cref).HasMaxLength(20).IsRequired();
            e.Property(i => i.Telefone).HasMaxLength(20);
            e.Property(i => i.Email).HasMaxLength(100);
            e.HasIndex(i => i.Cref).IsUnique();
        });

        // ---------- EXERCICIO ----------
        builder.Entity<Exercicio>(e =>
        {
            e.ToTable("EXERCICIO");
            e.Property(x => x.Nome).HasMaxLength(80).IsRequired();
            e.Property(x => x.GrupoMuscular).HasMaxLength(40).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(255);
        });

        // ---------- TREINO ----------
        builder.Entity<Treino>(e =>
        {
            e.ToTable("TREINO");
            e.Property(t => t.Nome).HasMaxLength(60).IsRequired();
            e.Property(t => t.Objetivo).HasMaxLength(255);
            e.Property(t => t.DataCriacao).HasColumnType("date");

            e.HasOne(t => t.Aluno)
                .WithMany(a => a.Treinos)
                .HasForeignKey(t => t.AlunoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(t => t.Instrutor)
                .WithMany(i => i.Treinos)
                .HasForeignKey(t => t.InstrutorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- ITEM_TREINO ----------
        builder.Entity<ItemTreino>(e =>
        {
            e.ToTable("ITEM_TREINO");
            e.Property(i => i.Repeticoes).HasMaxLength(20).IsRequired();
            e.Property(i => i.CargaKg).HasColumnType("decimal(6,2)");

            // Composição: o item não existe sem a ficha de treino.
            e.HasOne(i => i.Treino)
                .WithMany(t => t.Itens)
                .HasForeignKey(i => i.TreinoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Exercicio)
                .WithMany(x => x.Itens)
                .HasForeignKey(i => i.ExercicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        DadosIniciais.Configurar(builder);
    }
}
