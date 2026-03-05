namespace MotorCompra.Service.Infrastructure.Persistence;

public class CustodiaMasterEntity
{
    public long Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
