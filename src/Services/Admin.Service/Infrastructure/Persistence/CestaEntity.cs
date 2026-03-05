namespace Admin.Service.Infrastructure.Persistence;
public class CestaEntity
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataDesativacao { get; set; }
    public ICollection<ItemCestaEntity> Itens { get; set; } = new List<ItemCestaEntity>();
}
public class ItemCestaEntity
{
    public long Id { get; set; }
    public long CestaId { get; set; }
    public CestaEntity Cesta { get; set; } = null!;
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}
