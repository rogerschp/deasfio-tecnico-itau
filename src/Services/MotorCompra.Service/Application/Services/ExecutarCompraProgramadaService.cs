using Microsoft.Extensions.Logging;
using MotorCompra.Service.Application.Entities;
using MotorCompra.Service.Application.Exceptions;
using MotorCompra.Service.Application.Ports;
using Shared.Kafka;
using Shared.Contracts.Eventos;
namespace MotorCompra.Service.Application.Services;

public class ExecutarCompraProgramadaService : IExecutarCompraProgramadaService
{
    private const int TamanhoLotePadraoB3 = 100;
    private const decimal UmTerco = 1m / 3m;
    private const decimal AliquotaIrDedoDuro = 0.00005m;
    private readonly ICestaVigenteClient _cestaClient;
    private readonly IClientesAtivosClient _clientesClient;
    private readonly ICotacaoFechamentoClient _cotacaoClient;
    private readonly IRegistroDistribuicaoClient _registroDistribuicaoClient;
    private readonly IExecucaoCompraRepository _execucaoRepo;
    private readonly ICustodiaMasterRepository _custodiaRepo;
    private readonly IEventoIRPublisher _kafka;
    private readonly ILogger<ExecutarCompraProgramadaService> _logger;
    public ExecutarCompraProgramadaService(
        ICestaVigenteClient cestaClient,
        IClientesAtivosClient clientesClient,
        ICotacaoFechamentoClient cotacaoClient,
        IRegistroDistribuicaoClient registroDistribuicaoClient,
        IExecucaoCompraRepository execucaoRepo,
        ICustodiaMasterRepository custodiaRepo,
        IEventoIRPublisher kafka,
        ILogger<ExecutarCompraProgramadaService> logger)
    {
        _cestaClient = cestaClient;
        _clientesClient = clientesClient;
        _cotacaoClient = cotacaoClient;
        _registroDistribuicaoClient = registroDistribuicaoClient;
        _execucaoRepo = execucaoRepo;
        _custodiaRepo = custodiaRepo;
        _kafka = kafka;
        _logger = logger;
    }
    public async Task<ExecucaoCompra?> ExecutarAsync(DateOnly referenceDate, CancellationToken ct = default)
    {
        if (await _execucaoRepo.JaExecutouNaDataAsync(referenceDate, ct))
            throw new CompraJaExecutadaException(referenceDate);
        var basket = await _cestaClient.GetCestaVigenteAsync(ct);
        if (basket?.Itens == null || basket.Itens.Count == 0)
        {
            _logger.LogWarning("Motor 204: Nenhuma cesta vigente. Cadastre uma cesta via POST /api/admin/cesta.");
            return null;
        }
        var customers = await _clientesClient.GetClientesAtivosAsync(ct);
        if (customers.Count == 0)
        {
            _logger.LogWarning("Motor 204: Nenhum cliente ativo. Faça uma adesão via POST /api/clientes/adesao.");
            return null;
        }
        var tickerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < basket.Itens.Count; i++)
            tickerSet.Add(basket.Itens[i].Ticker);
        var tickers = new List<string>(tickerSet);
        var quotes = await _cotacaoClient.GetFechamentosAsync(tickers, ct);
        if (quotes.Count == 0)
        {
            _logger.LogWarning("Motor 204: Nenhuma cotação de fechamento para os tickers da cesta ({Tickers}). Importe um COTAHIST via POST /api/cotacoes/importar.", string.Join(", ", tickers));
            return null;
        }
        var priceByTicker = new Dictionary<string, decimal>(quotes.Count, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < quotes.Count; i++)
        {
            var c = quotes[i];
            priceByTicker[c.Ticker] = c.PrecoFechamento;
        }
        for (var i = 0; i < tickers.Count; i++)
        {
            var t = tickers[i];
            if (!priceByTicker.ContainsKey(t))
            {
                _logger.LogWarning("Motor 204: Cotação de fechamento não encontrada para o ticker {Ticker}. Verifique se o COTAHIST importado contém esse ativo.", t);
                return null;
            }
        }

        var contributions = new List<(long ClienteId, string Nome, string Cpf, long ContaGraficaId, decimal ValorData)>(customers.Count);
        decimal totalConsolidated = 0;
        for (var i = 0; i < customers.Count; i++)
        {
            var c = customers[i];
            var valorData = c.ValorMensal * UmTerco;
            contributions.Add((c.ClienteId, c.Nome, c.Cpf, c.ContaGraficaId, valorData));
            totalConsolidated += valorData;
        }
        if (totalConsolidated <= 0)
        {
            _logger.LogWarning("Motor 204: Total consolidado de aportes é zero (valor mensal dos clientes pode ser zero).");
            return null;
        }

        var valueByTicker = new Dictionary<string, decimal>(basket.Itens.Count, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < basket.Itens.Count; i++)
        {
            var item = basket.Itens[i];
            valueByTicker[item.Ticker] = totalConsolidated * (item.Percentual / 100m);
        }

        var masterBalances = await _custodiaRepo.GetSaldosPorTickerAsync(tickers, ct);
        var quantityToBuy = new Dictionary<string, int>();
        foreach (var item in basket.Itens)
        {
            var amount = valueByTicker[item.Ticker];
            var price = priceByTicker[item.Ticker];
            var qtyWithoutBalance = price > 0 ? (int)Math.Floor(amount / price) : 0;
            var balance = masterBalances.GetValueOrDefault(item.Ticker, 0);
            quantityToBuy[item.Ticker] = Math.Max(0, qtyWithoutBalance - balance);
        }

        var orders = new List<OrdemCompraItem>();
        var boughtByTicker = new Dictionary<string, int>();
        foreach (var item in basket.Itens)
        {
            var qty = quantityToBuy.GetValueOrDefault(item.Ticker, 0);
            var price = priceByTicker[item.Ticker];
            var (standardLot, fractional) = SepararLoteFracionario(qty);
            var details = new List<DetalheOrdemDto>();
            if (standardLot > 0)
                details.Add(new DetalheOrdemDto("LOTE_PADRAO", item.Ticker, standardLot));
            if (fractional > 0)
                details.Add(new DetalheOrdemDto("FRACIONARIO", item.Ticker + "F", fractional));
            var totalQty = standardLot + fractional;
            orders.Add(new OrdemCompraItem
            {
                Ticker = item.Ticker,
                QuantidadeTotal = totalQty,
                PrecoUnitario = price,
                ValorTotal = totalQty * price,
                Detalhes = details
            });
            boughtByTicker[item.Ticker] = totalQty;
        }

        var availableByTicker = new Dictionary<string, int>();
        foreach (var t in tickers)
            availableByTicker[t] = masterBalances.GetValueOrDefault(t, 0) + boughtByTicker.GetValueOrDefault(t, 0);

        var distributions = new List<DistribuicaoCliente>();
        foreach (var (customerId, name, cpf, _, contributionAmount) in contributions)
        {
            var proportion = totalConsolidated > 0 ? contributionAmount / totalConsolidated : 0;
            var assets = new List<AtivoDistribuidoDto>();
            foreach (var item in basket.Itens)
            {
                var available = availableByTicker.GetValueOrDefault(item.Ticker, 0);
                var customerQty = (int)Math.Floor(available * proportion);
                if (customerQty > 0)
                {
                    assets.Add(new AtivoDistribuidoDto(item.Ticker, customerQty));
                    availableByTicker[item.Ticker] = available - customerQty;
                }
            }
            distributions.Add(new DistribuicaoCliente
            {
                ClienteId = customerId,
                Nome = name,
                Cpf = cpf,
                ValorAporte = contributionAmount,
                Ativos = assets
            });
        }

        var execution = new ExecucaoCompra
        {
            DataReferencia = referenceDate,
            DataExecucao = DateTime.UtcNow,
            TotalConsolidado = totalConsolidated,
            TotalClientes = customers.Count,
            Ordens = orders,
            Distribuicoes = distributions
        };
        await _execucaoRepo.SalvarExecucaoAsync(execution, ct);

        var residuals = new List<(string Ticker, int Quantidade)>(tickers.Count);
        for (var i = 0; i < tickers.Count; i++)
            residuals.Add((tickers[i], availableByTicker.GetValueOrDefault(tickers[i], 0)));
        await _custodiaRepo.DefinirResiduosAsync(residuals, ct);

        var installment = referenceDate.Day switch { 5 => 1, 15 => 2, 25 => 3, _ => (int?)null };
        foreach (var dist in distributions)
        {
            var itemsWithPrice = new List<ItemDistribuicaoDto>(dist.Ativos.Count);
            for (var i = 0; i < dist.Ativos.Count; i++)
            {
                var a = dist.Ativos[i];
                itemsWithPrice.Add(new ItemDistribuicaoDto(a.Ticker, a.Quantidade, priceByTicker[a.Ticker]));
            }
            await _registroDistribuicaoClient.RegistrarDistribuicaoAsync(dist.ClienteId, execution.Id, itemsWithPrice, referenceDate, dist.ValorAporte, installment, ct);
            foreach (var asset in dist.Ativos)
            {
                var operationAmount = asset.Quantidade * priceByTicker[asset.Ticker];
                var irAmount = Math.Round(operationAmount * AliquotaIrDedoDuro, 2);
                try
                {
                    await _kafka.PublicarDedoDuroAsync(new EventoIRDedoDuro(
                        Tipo: "IR_DEDO_DURO",
                        ClienteId: dist.ClienteId,
                        Cpf: dist.Cpf,
                        Ticker: asset.Ticker,
                        TipoOperacao: "COMPRA",
                        Quantidade: asset.Quantidade,
                        PrecoUnitario: priceByTicker[asset.Ticker],
                        ValorOperacao: operationAmount,
                        Aliquota: AliquotaIrDedoDuro,
                        ValorIR: irAmount,
                        DataOperacao: execution.DataExecucao
                    ), ct);
                }
                catch (Exception ex)
                {
                    throw new KafkaIndisponivelException("Falha ao publicar IR dedo-duro no Kafka.", ex);
                }
            }
        }
        return execution;
    }
    private static (int LotePadrao, int Fracionario) SepararLoteFracionario(int quantity)
    {
        var lots = quantity / TamanhoLotePadraoB3;
        var fractionalPart = quantity % TamanhoLotePadraoB3;
        return (lots * TamanhoLotePadraoB3, fractionalPart);
    }
}
