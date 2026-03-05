using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MotorCompra.Service.Application.Ports;
namespace MotorCompra.Service.Infrastructure.Clients;
public class HttpClientesAtivosClient : IClientesAtivosClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public HttpClientesAtivosClient(HttpClient http, IOptions<MotorCompraServiceOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.ClientesServiceBaseUrl.TrimEnd('/') + "/");
    }
    public async Task<IReadOnlyList<ClienteAtivoDto>> GetClientesAtivosAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync("api/clientes/ativos", ct);
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<ClienteAtivoResponse>>(Json, ct);
            if (list == null) return Array.Empty<ClienteAtivoDto>();
            return list
                .Select(c => new ClienteAtivoDto(c.ClienteId, c.Nome ?? "", c.Cpf ?? "", c.ValorMensal, c.ContaGraficaId))
                .Where(c => c.ContaGraficaId > 0)
                .ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<ClienteAtivoDto>();
        }
    }

    private class ClienteAtivoResponse
    {
        public long ClienteId { get; set; }
        public string? Nome { get; set; }
        public string? Cpf { get; set; }
        public decimal ValorMensal { get; set; }
        public long ContaGraficaId { get; set; }
    }
}
