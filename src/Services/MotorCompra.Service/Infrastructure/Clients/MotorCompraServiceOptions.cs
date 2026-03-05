namespace MotorCompra.Service.Infrastructure.Clients;
public class MotorCompraServiceOptions
{
    public const string SectionName = "MotorCompra";
    public string AdminServiceBaseUrl { get; set; } = "http://localhost:5002";
    public string ClientesServiceBaseUrl { get; set; } = "http://localhost:5001";
    public string CotacaoServiceBaseUrl { get; set; } = "http://localhost:5003";
}
