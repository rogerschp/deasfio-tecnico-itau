using Rebalanceamento.Service.Application.Exceptions;
using Rebalanceamento.Service.Application.Ports;
using Shared.Contracts.Eventos;
using Shared.Kafka;
namespace Rebalanceamento.Service.Application.Services;

public class ExecutarRebalanceamentoService : IExecutarRebalanceamentoService
{
    private const decimal LimiteIsencaoVendasMes = 20_000m;
    private const decimal AliquotaIRVenda = 0.20m;
    private readonly IClientesRebalanceamentoClient _clientes;
    private readonly ICestaVigenteClient _cesta;
    private readonly ICotacaoFechamentoClient _cotacao;
    private readonly IVendaRebalanceamentoRepository _vendaRepo;
    private readonly IEventoIRPublisher _kafka;
    public ExecutarRebalanceamentoService(
        IClientesRebalanceamentoClient clientes,
        ICestaVigenteClient cesta,
        ICotacaoFechamentoClient cotacao,
        IVendaRebalanceamentoRepository vendaRepo,
        IEventoIRPublisher kafka)
    {
        _clientes = clientes;
        _cesta = cesta;
        _cotacao = cotacao;
        _vendaRepo = vendaRepo;
        _kafka = kafka;
    }
    public async Task<ResultadoRebalanceamentoDto?> ExecutarPorMudancaCestaAsync(
        IReadOnlyList<ItemCestaDto> previousBasket,
        IReadOnlyList<ItemCestaDto> newBasket,
        CancellationToken ct = default)
    {
        if (previousBasket == null || newBasket == null || previousBasket.Count == 0 || newBasket.Count == 0)
            return null;
        var customers = await _clientes.GetClientesAtivosAsync(ct);
        if (customers.Count == 0)
            return new ResultadoRebalanceamentoDto(DateTime.UtcNow, 0, 0, 0, new[] { "Nenhum cliente ativo." });
        var tickersBefore = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < previousBasket.Count; i++)
            tickersBefore.Add(previousBasket[i].Ticker);
        var tickersAfter = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < newBasket.Count; i++)
            tickersAfter.Add(newBasket[i].Ticker);
        var removed = new HashSet<string>(tickersBefore.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var t in tickersBefore)
            if (!tickersAfter.Contains(t)) removed.Add(t);
        var added = new HashSet<string>(tickersAfter.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var t in tickersAfter)
            if (!tickersBefore.Contains(t)) added.Add(t);
        var kept = new HashSet<string>(tickersBefore.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var t in tickersBefore)
            if (tickersAfter.Contains(t)) kept.Add(t);
        var allTickers = new List<string>(tickersBefore.Count + tickersAfter.Count);
        foreach (var t in tickersBefore) allTickers.Add(t);
        foreach (var t in tickersAfter)
            if (!tickersBefore.Contains(t)) allTickers.Add(t);
        var quotes = await _cotacao.GetFechamentosAsync(allTickers, ct);
        var priceByTicker = new Dictionary<string, decimal>(quotes.Count, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < quotes.Count; i++)
        {
            var c = quotes[i];
            priceByTicker[c.Ticker] = c.PrecoFechamento;
        }
        for (var i = 0; i < allTickers.Count; i++)
        {
            if (!priceByTicker.ContainsKey(allTickers[i]))
                return new ResultadoRebalanceamentoDto(DateTime.UtcNow, 0, 0, 0, new[] { "Cotação indisponível para algum ativo." });
        }
        var errors = new List<string>();
        var totalSales = 0;
        var totalPurchases = 0;
        foreach (var customer in customers)
        {
            var portfolio = await _clientes.GetCarteiraAsync(customer.ClienteId, ct);
            if (portfolio?.Posicoes == null || portfolio.Posicoes.Count == 0)
                continue;
            var positions = new Dictionary<string, PosicaoCustodiaDto>(portfolio.Posicoes.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < portfolio.Posicoes.Count; i++)
            {
                var p = portfolio.Posicoes[i];
                positions[p.Ticker] = p;
            }
            decimal portfolioValue = 0;
            foreach (var p in portfolio.Posicoes)
                portfolioValue += p.Quantidade * priceByTicker.GetValueOrDefault(p.Ticker, p.PrecoMedio);

            decimal salesProceeds = 0;
            var saleDetails = new List<DetalheVenda>();
            foreach (var ticker in removed)
            {
                if (!positions.TryGetValue(ticker, out var pos) || pos.Quantidade <= 0) continue;
                var price = priceByTicker[ticker];
                var res = await _clientes.VenderAsync(customer.ClienteId, ticker, pos.Quantidade, price, ct);
                if (res != null)
                {
                    salesProceeds += res.ValorVenda;
                    totalSales++;
                    var profit = res.Lucro;
                    await _vendaRepo.RegistrarVendaAsync(customer.ClienteId, customer.Cpf, ticker, pos.Quantidade, price, pos.PrecoMedio, profit, DateTime.UtcNow, ct);
                    saleDetails.Add(new DetalheVenda(ticker, pos.Quantidade, price, pos.PrecoMedio, profit));
                    positions.Remove(ticker);
                }
            }

            if (added.Count > 0 && salesProceeds > 0)
            {
                var addedItems = new List<ItemCestaDto>(added.Count);
                decimal sumPct = 0;
                for (var i = 0; i < newBasket.Count; i++)
                {
                    var it = newBasket[i];
                    if (added.Contains(it.Ticker))
                    {
                        addedItems.Add(it);
                        sumPct += it.Percentual;
                    }
                }
                if (sumPct > 0)
                {
                    foreach (var item in addedItems)
                    {
                        var targetAmount = salesProceeds * (item.Percentual / sumPct);
                        var price = priceByTicker[item.Ticker];
                        if (price <= 0) continue;
                        var qty = (int)Math.Floor(targetAmount / price);
                        if (qty <= 0) continue;
                        if (await _clientes.ComprarAsync(customer.ClienteId, item.Ticker, qty, price, ct))
                        {
                            totalPurchases++;
                            if (!positions.ContainsKey(item.Ticker))
                                positions[item.Ticker] = new PosicaoCustodiaDto(item.Ticker, 0, price);
                            var at = positions[item.Ticker];
                            positions[item.Ticker] = new PosicaoCustodiaDto(item.Ticker, at.Quantidade + qty, at.PrecoMedio);
                        }
                    }
                }
            }

            portfolioValue = 0;
            foreach (var (_, pos) in positions)
                portfolioValue += pos.Quantidade * priceByTicker.GetValueOrDefault(pos.Ticker, pos.PrecoMedio);

            var newBasketDict = new Dictionary<string, decimal>(newBasket.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < newBasket.Count; i++)
            {
                var it = newBasket[i];
                newBasketDict[it.Ticker] = it.Percentual;
            }
            foreach (var ticker in kept)
            {
                if (!newBasketDict.TryGetValue(ticker, out var targetPct)) continue;
                if (!positions.TryGetValue(ticker, out var pos)) continue;
                var price = priceByTicker[ticker];
                var currentAssetValue = pos.Quantidade * price;
                var targetValue = portfolioValue * (targetPct / 100m);
                var diff = currentAssetValue - targetValue;
                if (diff > price * 0.5m)
                {
                    var qtyToSell = (int)Math.Floor(diff / price);
                    if (qtyToSell > 0 && qtyToSell < pos.Quantidade)
                    {
                        var res = await _clientes.VenderAsync(customer.ClienteId, ticker, qtyToSell, price, ct);
                        if (res != null)
                        {
                            totalSales++;
                            await _vendaRepo.RegistrarVendaAsync(customer.ClienteId, customer.Cpf, ticker, qtyToSell, price, pos.PrecoMedio, res.Lucro, DateTime.UtcNow, ct);
                            saleDetails.Add(new DetalheVenda(ticker, qtyToSell, price, pos.PrecoMedio, res.Lucro));
                            positions[ticker] = new PosicaoCustodiaDto(ticker, pos.Quantidade - qtyToSell, pos.PrecoMedio);
                        }
                    }
                }
                else if (diff < -price * 0.5m)
                {
                    var qtyToBuy = (int)Math.Floor(-diff / price);
                    if (qtyToBuy > 0 && await _clientes.ComprarAsync(customer.ClienteId, ticker, qtyToBuy, price, ct))
                    {
                        totalPurchases++;
                        var at = positions[ticker];
                        positions[ticker] = new PosicaoCustodiaDto(ticker, at.Quantidade + qtyToBuy, at.PrecoMedio);
                    }
                }
            }

            var now = DateTime.UtcNow;
            var (totalSalesInMonth, netProfitInMonth) = await _vendaRepo.GetTotalVendasELucroClienteNoMesAsync(customer.ClienteId, now.Year, now.Month, ct);
            if (totalSalesInMonth > LimiteIsencaoVendasMes && netProfitInMonth > 0)
            {
                var irAmount = Math.Round(netProfitInMonth * AliquotaIRVenda, 2);
                var evento = new EventoIRVenda(
                    "IR_VENDA",
                    customer.ClienteId,
                    customer.Cpf,
                    $"{now.Year:D4}-{now.Month:D2}",
                    totalSalesInMonth,
                    netProfitInMonth,
                    AliquotaIRVenda,
                    irAmount,
                    saleDetails,
                    now);
                try
                {
                    await _kafka.PublicarIRVendaAsync(evento, ct);
                }
                catch (Exception ex)
                {
                    throw new KafkaIndisponivelException("Falha ao publicar IR venda no Kafka.", ex);
                }
            }
        }
        return new ResultadoRebalanceamentoDto(
            DateTime.UtcNow,
            customers.Count,
            totalSales,
            totalPurchases,
            errors);
    }
    public async Task<ResultadoRebalanceamentoDto?> ExecutarPorDesvioAsync(decimal thresholdPercentage = RebalanceamentoConstants.LimiarDesvioPadrao, CancellationToken ct = default)
    {
        var basket = await _cesta.GetCestaVigenteAsync(ct);
        if (basket?.Itens == null || basket.Itens.Count == 0)
            return new ResultadoRebalanceamentoDto(DateTime.UtcNow, 0, 0, 0, new[] { "Cesta vigente não encontrada." });
        var customers = await _clientes.GetClientesAtivosAsync(ct);
        if (customers.Count == 0)
            return new ResultadoRebalanceamentoDto(DateTime.UtcNow, 0, 0, 0, new[] { "Nenhum cliente ativo." });
        var tickerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < basket.Itens.Count; i++)
            tickerSet.Add(basket.Itens[i].Ticker);
        var tickers = new List<string>(tickerSet);
        var quotes = await _cotacao.GetFechamentosAsync(tickers, ct);
        var priceByTicker = new Dictionary<string, decimal>(quotes.Count, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < quotes.Count; i++)
        {
            var c = quotes[i];
            priceByTicker[c.Ticker] = c.PrecoFechamento;
        }
        for (var i = 0; i < tickers.Count; i++)
        {
            if (!priceByTicker.ContainsKey(tickers[i]))
                return new ResultadoRebalanceamentoDto(DateTime.UtcNow, 0, 0, 0, new[] { "Cotação indisponível." });
        }
        var basketDict = new Dictionary<string, decimal>(basket.Itens.Count, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < basket.Itens.Count; i++)
        {
            var item = basket.Itens[i];
            basketDict[item.Ticker] = item.Percentual;
        }
        var errors = new List<string>();
        var totalSales = 0;
        var totalPurchases = 0;
        foreach (var customer in customers)
        {
            var portfolio = await _clientes.GetCarteiraAsync(customer.ClienteId, ct);
            if (portfolio?.Posicoes == null || portfolio.Posicoes.Count == 0)
                continue;
            decimal totalValue = 0;
            var valueByTicker = new Dictionary<string, decimal>(portfolio.Posicoes.Count, StringComparer.OrdinalIgnoreCase);
            var positionByTicker = new Dictionary<string, PosicaoCustodiaDto>(portfolio.Posicoes.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < portfolio.Posicoes.Count; i++)
            {
                var p = portfolio.Posicoes[i];
                var price = priceByTicker.GetValueOrDefault(p.Ticker, p.PrecoMedio);
                var value = p.Quantidade * price;
                valueByTicker[p.Ticker] = value;
                positionByTicker[p.Ticker] = p;
                totalValue += value;
            }
            if (totalValue <= 0) continue;

            for (var idx = 0; idx < basket.Itens.Count; idx++)
            {
                var item = basket.Itens[idx];
                var currentValue = valueByTicker.GetValueOrDefault(item.Ticker, 0);
                var currentPct = totalValue > 0 ? (currentValue / totalValue) * 100m : 0;
                var desvio = currentPct - item.Percentual;
                var price = priceByTicker[item.Ticker];
                if (price <= 0) continue;
                if (desvio > thresholdPercentage)
                {
                    var excessValue = totalValue * (desvio / 100m);
                    var qtyToSell = (int)Math.Floor(excessValue / price);
                    if (positionByTicker.TryGetValue(item.Ticker, out var position) && qtyToSell > 0 && qtyToSell <= position.Quantidade)
                    {
                        var res = await _clientes.VenderAsync(customer.ClienteId, item.Ticker, qtyToSell, price, ct);
                        if (res != null)
                        {
                            totalSales++;
                            await _vendaRepo.RegistrarVendaAsync(customer.ClienteId, customer.Cpf, item.Ticker, qtyToSell, price, position.PrecoMedio, res.Lucro, DateTime.UtcNow, ct);
                        }
                    }
                }
                else if (desvio < -thresholdPercentage)
                {
                    var deficitValue = totalValue * (-desvio / 100m);
                    var qtyToBuy = (int)Math.Floor(deficitValue / price);
                    if (qtyToBuy > 0 && await _clientes.ComprarAsync(customer.ClienteId, item.Ticker, qtyToBuy, price, ct))
                        totalPurchases++;
                }
            }

            var now = DateTime.UtcNow;
            var (totalSalesInMonth, netProfitInMonth) = await _vendaRepo.GetTotalVendasELucroClienteNoMesAsync(customer.ClienteId, now.Year, now.Month, ct);
            if (totalSalesInMonth > LimiteIsencaoVendasMes && netProfitInMonth > 0)
            {
                var irAmount = Math.Round(netProfitInMonth * AliquotaIRVenda, 2);
                var evento = new EventoIRVenda(
                    "IR_VENDA",
                    customer.ClienteId,
                    customer.Cpf,
                    $"{now.Year:D4}-{now.Month:D2}",
                    totalSalesInMonth,
                    netProfitInMonth,
                    AliquotaIRVenda,
                    irAmount,
                    Array.Empty<DetalheVenda>(),
                    now);
                try
                {
                    await _kafka.PublicarIRVendaAsync(evento, ct);
                }
                catch (Exception ex)
                {
                    throw new KafkaIndisponivelException("Falha ao publicar IR venda no Kafka.", ex);
                }
            }
        }
        return new ResultadoRebalanceamentoDto(DateTime.UtcNow, customers.Count, totalSales, totalPurchases, errors);
    }
}
