using System.Net.Http.Json;
using System.Text.Json;
using Admin.Service.Application.Ports;
using Microsoft.Extensions.Options;
namespace Admin.Service.Infrastructure.Clients;
public class HttpCotacaoFechamentoClient : ICotacaoFechamentoClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public HttpCotacaoFechamentoClient(HttpClient http, IOptions<AdminServiceOptions> options)
    {
        _http = http;
        var url = options.Value.CotacaoServiceBaseUrl?.TrimEnd('/') ?? "http://localhost:5003";
        _http.BaseAddress = new Uri(url + "/");
    }
    public async Task<IReadOnlyList<CotacaoFechamentoDto>> GetFechamentosAsync(IReadOnlyList<string> tickers, CancellationToken ct = default)
    {
        if (tickers.Count == 0) return Array.Empty<CotacaoFechamentoDto>();
        var query = "api/cotacoes/fechamento?tickers=" + Uri.EscapeDataString(string.Join(",", tickers));
        try
        {
            var response = await _http.GetAsync(query, ct);
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<CotacaoFechamentoDto>>(Json, ct);
            return list ?? new List<CotacaoFechamentoDto>();
        }
        catch
        {
            return Array.Empty<CotacaoFechamentoDto>();
        }
    }
}
