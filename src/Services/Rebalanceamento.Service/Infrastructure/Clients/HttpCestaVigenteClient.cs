using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Rebalanceamento.Service.Application.Ports;
namespace Rebalanceamento.Service.Infrastructure.Clients;
public class HttpCestaVigenteClient : ICestaVigenteClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public HttpCestaVigenteClient(HttpClient http, IOptions<RebalanceamentoServiceOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.AdminServiceBaseUrl.TrimEnd('/') + "/");
    }
    public async Task<CestaVigenteDto?> GetCestaVigenteAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync("api/admin/cesta/vigente", ct);
            if (!response.IsSuccessStatusCode) return null;
            var raw = await response.Content.ReadFromJsonAsync<CestaVigenteResponse>(Json, ct);
            if (raw?.Itens == null || raw.Itens.Count == 0) return null;
            var itens = raw.Itens.Select(i => new ItemCestaDto(i.Ticker, i.Percentual)).ToList();
            return new CestaVigenteDto(raw.CestaId, raw.Nome ?? "", itens);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
    private class CestaVigenteResponse
    {
        public long CestaId { get; set; }
        public string? Nome { get; set; }
        public List<ItemResponse>? Itens { get; set; }
    }
    private class ItemResponse
    {
        public string Ticker { get; set; } = "";
        public decimal Percentual { get; set; }
    }
}
