using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Rebalanceamento.Service.Application.Ports;
namespace Rebalanceamento.Service.Infrastructure.Clients;
public class HttpCotacaoFechamentoClient : ICotacaoFechamentoClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public HttpCotacaoFechamentoClient(HttpClient http, IOptions<RebalanceamentoServiceOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.CotacaoServiceBaseUrl.TrimEnd('/') + "/");
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
        catch (HttpRequestException)
        {
            return Array.Empty<CotacaoFechamentoDto>();
        }
    }
}
