using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MotorCompra.Service.Application.Ports;
namespace MotorCompra.Service.Infrastructure.Clients;
public class HttpRegistroDistribuicaoClient : IRegistroDistribuicaoClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public HttpRegistroDistribuicaoClient(HttpClient http, IOptions<MotorCompraServiceOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.ClientesServiceBaseUrl.TrimEnd('/') + "/");
    }
    public async Task RegistrarDistribuicaoAsync(long clienteId, long execucaoId, IReadOnlyList<ItemDistribuicaoDto> itens, DateOnly? dataAporte = null, decimal? valorAporte = null, int? parcela = null, CancellationToken ct = default)
    {
        if (itens.Count == 0) return;
        var payload = new { execucaoId, clienteId, itens, dataAporte, valorAporte, parcela };
        var response = await _http.PostAsJsonAsync("api/clientes/distribuicao", payload, Json, ct);
        response.EnsureSuccessStatusCode();
    }
}
