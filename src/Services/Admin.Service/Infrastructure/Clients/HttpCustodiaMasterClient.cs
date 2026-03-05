using System.Net.Http.Json;
using System.Text.Json;
using Admin.Service.Application.Ports;
using Microsoft.Extensions.Options;
namespace Admin.Service.Infrastructure.Clients;
public class HttpCustodiaMasterClient : ICustodiaMasterClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public HttpCustodiaMasterClient(HttpClient http, IOptions<AdminServiceOptions> options)
    {
        _http = http;
        var url = options.Value.MotorServiceBaseUrl?.TrimEnd('/') ?? "http://localhost:5004";
        _http.BaseAddress = new Uri(url + "/");
    }
    public async Task<CustodiaMasterResponseDto?> GetCustodiaMasterAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync("api/motor/custodia-master", ct);
            if (!response.IsSuccessStatusCode) return null;
            var raw = await response.Content.ReadFromJsonAsync<CustodiaMasterApiResponse>(Json, ct);
            if (raw?.ContaMaster == null || raw.Custodia == null) return null;
            return new CustodiaMasterResponseDto(
                new ContaMasterDto(raw.ContaMaster.Id, raw.ContaMaster.NumeroConta ?? "", raw.ContaMaster.Tipo ?? ""),
                raw.Custodia.Select(c => new CustodiaMasterItemDto(c.Ticker, c.Quantidade, c.PrecoMedio, c.ValorAtual, c.Origem ?? "")).ToList(),
                raw.ValorTotalResiduo);
        }
        catch
        {
            return null;
        }
    }
    private class CustodiaMasterApiResponse
    {
        public ContaMasterApi? ContaMaster { get; set; }
        public List<CustodiaItemApi>? Custodia { get; set; }
        public decimal ValorTotalResiduo { get; set; }
    }
    private class ContaMasterApi
    {
        public long Id { get; set; }
        public string? NumeroConta { get; set; }
        public string? Tipo { get; set; }
    }
    private class CustodiaItemApi
    {
        public string Ticker { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal ValorAtual { get; set; }
        public string? Origem { get; set; }
    }
}
