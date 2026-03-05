namespace Admin.Service.Application.Entities;

public class Cesta
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataDesativacao { get; set; }
    public IReadOnlyList<ItemCesta> Itens { get; set; } = [];
}
public class ItemCesta
{
    public long Id { get; set; }
    public long CestaId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}
