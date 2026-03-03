namespace MotorCompra.Service.Infrastructure.Persistence;

/// <summary>
/// Saldo remanescente na custódia master por ticker (resíduos de distribuições).
/// </summary>
public class CustodiaMasterEntity
{
    public long Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
