namespace Rebalanceamento.Service.Infrastructure;
public class RebalanceamentoServiceOptions
{
    public const string SectionName = "Rebalanceamento";
    public string AdminServiceBaseUrl { get; set; } = "http://localhost:5002";
    public string ClientesServiceBaseUrl { get; set; } = "http://localhost:5001";
    public string CotacaoServiceBaseUrl { get; set; } = "http://localhost:5003";
}
