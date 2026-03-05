namespace Clientes.Service.Infrastructure.Persistence;
public class AporteEntity
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public DateOnly DataAporte { get; set; }
    public decimal Valor { get; set; }
    public int Parcela { get; set; }
}
