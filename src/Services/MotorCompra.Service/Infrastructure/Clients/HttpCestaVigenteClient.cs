using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MotorCompra.Service.Application.Ports;

namespace MotorCompra.Service.Infrastructure.Clients;

public class HttpCestaVigenteClient : ICestaVigenteClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public HttpCestaVigenteClient(HttpClient http, IOptions<MotorCompraServiceOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.AdminServiceBaseUrl.TrimEnd('/') + "/");
    }

    public async Task<CestaVigenteDto?> GetCestaVigenteAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync("api/admin/cesta/vigente", ct);
            response.EnsureSuccessStatusCode();
            var raw = await response.Content.ReadFromJsonAsync<CestaVigenteResponse>(_json, ct);
            if (raw?.Itens == null || raw.Itens.Count == 0) return null;
            return new CestaVigenteDto(raw.CestaId, raw.Nome ?? "", raw.Itens.Select(i => new ItemCestaDto(i.Ticker, i.Percentual)).ToList());
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
        public List<ItemCestaResponse>? Itens { get; set; }
    }

    private class ItemCestaResponse
    {
        public string Ticker { get; set; } = "";
        public decimal Percentual { get; set; }
    }
}
