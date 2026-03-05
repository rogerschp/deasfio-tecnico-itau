namespace MotorCompra.Service.Infrastructure.Persistence;
public class ExecucaoCompraEntity
{
    public long Id { get; set; }
    public DateOnly DataReferencia { get; set; }
    public DateTime DataExecucao { get; set; }
    public decimal TotalConsolidado { get; set; }
    public int TotalClientes { get; set; }
    public ICollection<OrdemCompraEntity> Ordens { get; set; } = new List<OrdemCompraEntity>();
    public ICollection<DistribuicaoEntity> Distribuicoes { get; set; } = new List<DistribuicaoEntity>();
}
public class OrdemCompraEntity
{
    public long Id { get; set; }
    public long ExecucaoCompraId { get; set; }
    public ExecucaoCompraEntity ExecucaoCompra { get; set; } = null!;
    public string Ticker { get; set; } = string.Empty;
    public int QuantidadeTotal { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string DetalhesJson { get; set; } = "[]";
}
public class DistribuicaoEntity
{
    public long Id { get; set; }
    public long ExecucaoCompraId { get; set; }
    public ExecucaoCompraEntity ExecucaoCompra { get; set; } = null!;
    public long ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public decimal ValorAporte { get; set; }
    public string AtivosJson { get; set; } = "[]";
}
