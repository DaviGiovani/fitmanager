using System.ComponentModel.DataAnnotations;

namespace FitManager.Models;

/// <summary>Ficha de treino de um aluno, elaborada por um instrutor.</summary>
public class Treino
{
    public int Id { get; set; }

    public int AlunoId { get; set; }

    public int InstrutorId { get; set; }

    [Required]
    [StringLength(60)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Objetivo { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de criação")]
    public DateTime DataCriacao { get; set; } = DateTime.Today;

    public bool Ativo { get; set; } = true;

    public Aluno? Aluno { get; set; }

    public Instrutor? Instrutor { get; set; }

    public ICollection<ItemTreino> Itens { get; set; } = new List<ItemTreino>();

    /// <summary>Adiciona o exercício ao final da ficha, definindo a ordem automaticamente.</summary>
    public void AdicionarItem(ItemTreino item)
    {
        item.Ordem = Itens.Count + 1;
        Itens.Add(item);
    }
}
