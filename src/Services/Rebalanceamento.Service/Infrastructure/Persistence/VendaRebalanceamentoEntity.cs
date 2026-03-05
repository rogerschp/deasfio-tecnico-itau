namespace Rebalanceamento.Service.Infrastructure.Persistence;
public class VendaRebalanceamentoEntity
{
    public long Id { get; set; }
    public long ClienteId { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal Lucro { get; set; }
    public DateTime DataExecucao { get; set; }
}
