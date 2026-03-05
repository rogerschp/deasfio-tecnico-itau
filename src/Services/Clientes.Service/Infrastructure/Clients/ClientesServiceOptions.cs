namespace Clientes.Service.Infrastructure.Clients;
public class ClientesServiceOptions
{
    public const string SectionName = "Clientes";
    public string CotacaoServiceBaseUrl { get; set; } = "http://localhost:5003";
}
