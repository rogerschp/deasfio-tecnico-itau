using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MotorCompra.Service.Application.Ports;
namespace MotorCompra.Service.Infrastructure.Clients;
public class HttpRegistroDistribuicaoClient : IRegistroDistribuicaoClient
{
    private readonly HttpClient _http;
    private readonly Uri _distribuicaoUrl;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public HttpRegistroDistribuicaoClient(HttpClient http, IOptions<MotorCompraServiceOptions> options)
    {
        _http = http;
        var baseUrl = options.Value.ClientesServiceBaseUrl.TrimEnd('/');
        _distribuicaoUrl = new Uri(new Uri(baseUrl + "/"), "api/internal/clientes/distribuicao");
    }

    public async Task RegistrarDistribuicaoAsync(long clienteId, long execucaoId, IReadOnlyList<ItemDistribuicaoDto> itens, DateOnly? dataAporte = null, decimal? valorAporte = null, int? parcela = null, CancellationToken ct = default)
    {
        if (itens.Count == 0) return;
        var payload = new { execucaoId, clienteId, itens, dataAporte, valorAporte, parcela };
        var response = await _http.PostAsJsonAsync(_distribuicaoUrl, payload, Json, ct);
        response.EnsureSuccessStatusCode();
    }
}
