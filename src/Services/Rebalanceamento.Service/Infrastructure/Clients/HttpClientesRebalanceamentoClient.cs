using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Rebalanceamento.Service.Application.Ports;
namespace Rebalanceamento.Service.Infrastructure.Clients;
public class HttpClientesRebalanceamentoClient : IClientesRebalanceamentoClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public HttpClientesRebalanceamentoClient(HttpClient http, IOptions<RebalanceamentoServiceOptions> options)
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
                .ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<ClienteAtivoDto>();
        }
    }
    public async Task<CarteiraClienteDto?> GetCarteiraAsync(long clienteId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"api/clientes/{clienteId}/carteira", ct);
            if (!response.IsSuccessStatusCode) return null;
            var raw = await response.Content.ReadFromJsonAsync<CarteiraApiResponse>(Json, ct);
            if (raw?.Ativos == null) return null;
            var posicoes = raw.Ativos.Select(a => new PosicaoCustodiaDto(a.Ticker, a.Quantidade, a.PrecoMedio)).ToList();
            return new CarteiraClienteDto(raw.ClienteId, posicoes);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
    public async Task<VendaCustodiaResultDto?> VenderAsync(long clienteId, string ticker, int quantidade, decimal precoVenda, CancellationToken ct = default)
    {
        try
        {
            var body = new { Ticker = ticker, Quantidade = quantidade, PrecoVenda = precoVenda };
            var response = await _http.PostAsJsonAsync($"api/internal/clientes/{clienteId}/custodia/venda", body, Json, ct);
            if (!response.IsSuccessStatusCode) return null;
            var raw = await response.Content.ReadFromJsonAsync<VendaResultResponse>(Json, ct);
            return raw != null ? new VendaCustodiaResultDto(raw.ValorVenda, raw.Lucro) : null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
    public async Task<bool> ComprarAsync(long clienteId, string ticker, int quantidade, decimal precoUnitario, CancellationToken ct = default)
    {
        try
        {
            var body = new { Ticker = ticker, Quantidade = quantidade, PrecoUnitario = precoUnitario };
            var response = await _http.PostAsJsonAsync($"api/internal/clientes/{clienteId}/custodia/compra", body, Json, ct);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
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
    private class CarteiraApiResponse
    {
        public long ClienteId { get; set; }
        public List<AtivoResponse>? Ativos { get; set; }
    }
    private class AtivoResponse
    {
        public string Ticker { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
    }
    private class VendaResultResponse
    {
        public decimal ValorVenda { get; set; }
        public decimal Lucro { get; set; }
    }
}
