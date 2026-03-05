using Admin.Service.Application.DTOs;
using Admin.Service.Application.Entities;
using Admin.Service.Application.Ports;
namespace Admin.Service.Application.Services;
public class CestaAppService : ICestaAppService
{
    private const int QuantidadeAtivosCesta = 5;
    private const decimal SomaPercentualEsperada = 100m;
    private readonly ICestaRepository _repository;
    private readonly ICotacaoFechamentoClient? _cotacaoClient;
    public CestaAppService(ICestaRepository repository, ICotacaoFechamentoClient? cotacaoClient = null)
    {
        _repository = repository;
        _cotacaoClient = cotacaoClient;
    }
    public async Task<CestaResponseDto> CadastrarOuAlterarAsync(string name, IReadOnlyList<(string Ticker, decimal Percentual)> items, CancellationToken ct = default)
    {
        if (items.Count != QuantidadeAtivosCesta)
            throw new ArgumentException($"A cesta deve conter exatamente {QuantidadeAtivosCesta} ativos. Quantidade informada: {items.Count}.", nameof(items));
        var sum = items.Sum(i => i.Percentual);
        if (Math.Abs(sum - SomaPercentualEsperada) > 0.001m)
            throw new ArgumentException($"A soma dos percentuais deve ser exatamente 100%. Soma atual: {sum}%.");
        if (items.Any(i => i.Percentual <= 0))
            throw new ArgumentException("Cada percentual deve ser maior que 0%.");
        var deactivationDate = DateTime.UtcNow;
        var previous = await _repository.GetAtivaAsync(ct);
        if (previous != null)
            await _repository.DesativarAsync(previous.Id, deactivationDate, ct);
        var cesta = new Cesta
        {
            Nome = name,
            Ativa = true,
            DataCriacao = DateTime.UtcNow,
            Itens = items.Select(i => new ItemCesta { Ticker = i.Ticker.Trim().ToUpperInvariant(), Percentual = i.Percentual }).ToList()
        };
        cesta = await _repository.SalvarAsync(cesta, ct);
        var removedAssets = previous?.Itens.Select(x => x.Ticker).Except(cesta.Itens.Select(x => x.Ticker)).ToList() ?? new List<string>();
        var addedAssets = cesta.Itens.Select(x => x.Ticker).Except(previous?.Itens.Select(x => x.Ticker) ?? Enumerable.Empty<string>()).ToList();
        var message = previous is null
            ? "Primeira cesta cadastrada com sucesso."
            : $"Cesta atualizada. Rebalanceamento disparado para clientes ativos.";
        return new CestaResponseDto(
            cesta.Id,
            cesta.Nome,
            true,
            cesta.DataCriacao,
            cesta.Itens.Select(i => new ItemCestaDto(i.Ticker, i.Percentual)).ToList(),
            RebalanceamentoDisparado: previous != null,
            CestaAnteriorDesativada: previous is null ? null : new CestaDesativadaDto(previous.Id, previous.Nome, deactivationDate),
            AtivosRemovidos: removedAssets,
            AtivosAdicionados: addedAssets,
            Mensagem: message);
    }
    public async Task<CestaResponseDto?> GetAtualAsync(CancellationToken ct = default)
    {
        var cesta = await _repository.GetAtivaAsync(ct);
        if (cesta is null) return null;
        if (_cotacaoClient != null && cesta.Itens.Count > 0)
        {
            var tickers = new List<string>(cesta.Itens.Count);
            for (var i = 0; i < cesta.Itens.Count; i++)
                tickers.Add(cesta.Itens[i].Ticker);
            var quotes = await _cotacaoClient.GetFechamentosAsync(tickers, ct);
            var priceByTicker = new Dictionary<string, decimal>(quotes.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < quotes.Count; i++)
            {
                var c = quotes[i];
                priceByTicker[c.Ticker] = c.PrecoFechamento;
            }
            var items = new List<ItemCestaDto>(cesta.Itens.Count);
            for (var i = 0; i < cesta.Itens.Count; i++)
            {
                var it = cesta.Itens[i];
                var cotacao = priceByTicker.TryGetValue(it.Ticker, out var preco) ? preco : (decimal?)null;
                items.Add(new ItemCestaDto(it.Ticker, it.Percentual, cotacao));
            }
            return new CestaResponseDto(cesta.Id, cesta.Nome, cesta.Ativa, cesta.DataCriacao, items);
        }
        return ToDto(cesta);
    }
    public async Task<CestaResponseDto?> GetVigenteAsync(CancellationToken ct = default) => await GetAtualAsync(ct);
    public async Task<CestaHistoricoDto> GetHistoricoAsync(CancellationToken ct = default)
    {
        var baskets = await _repository.GetHistoricoAsync(ct);
        var historyItems = new List<CestaHistoricoItemDto>(baskets.Count);
        for (var i = 0; i < baskets.Count; i++)
        {
            var c = baskets[i];
            var itens = new List<ItemCestaDto>(c.Itens.Count);
            for (var j = 0; j < c.Itens.Count; j++)
            {
                var it = c.Itens[j];
                itens.Add(new ItemCestaDto(it.Ticker, it.Percentual));
            }
            historyItems.Add(new CestaHistoricoItemDto(c.Id, c.Nome, c.Ativa, c.DataCriacao, c.DataDesativacao, itens));
        }
        return new CestaHistoricoDto(historyItems);
    }
    private static CestaResponseDto ToDto(Cesta c)
    {
        var itens = new List<ItemCestaDto>(c.Itens.Count);
        for (var i = 0; i < c.Itens.Count; i++)
        {
            var it = c.Itens[i];
            itens.Add(new ItemCestaDto(it.Ticker, it.Percentual));
        }
        return new CestaResponseDto(c.Id, c.Nome, c.Ativa, c.DataCriacao, itens);
    }
}
