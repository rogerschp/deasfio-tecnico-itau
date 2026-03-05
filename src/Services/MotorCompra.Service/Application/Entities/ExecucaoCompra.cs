namespace MotorCompra.Service.Application.Entities;

public class ExecucaoCompra
{
    public long Id { get; set; }
    public DateOnly DataReferencia { get; set; }
    public DateTime DataExecucao { get; set; }
    public decimal TotalConsolidado { get; set; }
    public int TotalClientes { get; set; }
    public IReadOnlyList<OrdemCompraItem> Ordens { get; set; } = [];
    public IReadOnlyList<DistribuicaoCliente> Distribuicoes { get; set; } = [];
}
public class OrdemCompraItem
{
    public string Ticker { get; set; } = string.Empty;
    public int QuantidadeTotal { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public IReadOnlyList<DetalheOrdemDto> Detalhes { get; set; } = [];
}
public record DetalheOrdemDto(string Tipo, string Ticker, int Quantidade);
public class DistribuicaoCliente
{
    public long ClienteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public decimal ValorAporte { get; set; }
    public IReadOnlyList<AtivoDistribuidoDto> Ativos { get; set; } = [];
}
public record AtivoDistribuidoDto(string Ticker, int Quantidade);
