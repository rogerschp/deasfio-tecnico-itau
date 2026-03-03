using MotorCompra.Service.Application.Entities;
using MotorCompra.Service.Application.Ports;
using Shared.Kafka;
using Shared.Contracts.Eventos;

namespace MotorCompra.Service.Application.Services;

/// <summary>
/// Orquestra o fluxo do motor de compra programada (RN-020 a RN-040, Docs/regras-negocio-detalhadas.md).
/// </summary>
public class ExecutarCompraProgramadaService : IExecutarCompraProgramadaService
{
    private const decimal UmTerco = 1m / 3m;
    private const decimal AliquotaIrDedoDuro = 0.00005m; // 0,005%

    private readonly ICestaVigenteClient _cestaClient;
    private readonly IClientesAtivosClient _clientesClient;
    private readonly ICotacaoFechamentoClient _cotacaoClient;
    private readonly IRegistroDistribuicaoClient _registroDistribuicaoClient;
    private readonly IExecucaoCompraRepository _execucaoRepo;
    private readonly ICustodiaMasterRepository _custodiaRepo;
    private readonly IEventoIRPublisher _kafka;

    public ExecutarCompraProgramadaService(
        ICestaVigenteClient cestaClient,
        IClientesAtivosClient clientesClient,
        ICotacaoFechamentoClient cotacaoClient,
        IRegistroDistribuicaoClient registroDistribuicaoClient,
        IExecucaoCompraRepository execucaoRepo,
        ICustodiaMasterRepository custodiaRepo,
        IEventoIRPublisher kafka)
    {
        _cestaClient = cestaClient;
        _clientesClient = clientesClient;
        _cotacaoClient = cotacaoClient;
        _registroDistribuicaoClient = registroDistribuicaoClient;
        _execucaoRepo = execucaoRepo;
        _custodiaRepo = custodiaRepo;
        _kafka = kafka;
    }

    public async Task<ExecucaoCompra?> ExecutarAsync(DateOnly dataReferencia, CancellationToken ct = default)
    {
        if (await _execucaoRepo.JaExecutouNaDataAsync(dataReferencia, ct))
            return null;

        var cesta = await _cestaClient.GetCestaVigenteAsync(ct);
        if (cesta?.Itens == null || cesta.Itens.Count == 0)
            return null;

        var clientes = await _clientesClient.GetClientesAtivosAsync(ct);
        if (clientes.Count == 0)
            return null;

        var tickers = cesta.Itens.Select(i => i.Ticker).Distinct().ToList();
        var cotacoes = await _cotacaoClient.GetFechamentosAsync(tickers, ct);
        if (cotacoes.Count == 0)
            return null;

        var precoPorTicker = cotacoes.ToDictionary(c => c.Ticker, c => c.PrecoFechamento);
        foreach (var t in tickers)
            if (!precoPorTicker.ContainsKey(t))
                return null;

        // 1) Agrupamento: 1/3 do valor mensal por cliente (RN-025, RN-026)
        var aportes = clientes.Select(c => (c.ClienteId, c.Nome, c.Cpf, c.ContaGraficaId, ValorData: c.ValorMensal * UmTerco)).ToList();
        var totalConsolidado = aportes.Sum(a => a.ValorData);
        if (totalConsolidado <= 0)
            return null;

        // 2) Valor por ativo segundo percentual da cesta
        var valorPorTicker = cesta.Itens.ToDictionary(i => i.Ticker, i => totalConsolidado * (i.Percentual / 100m));

        // 3) Saldo custódia master e quantidade a comprar (RN-028, RN-029, RN-030)
        var saldosMaster = await _custodiaRepo.GetSaldosPorTickerAsync(tickers, ct);
        var quantidadeAComprar = new Dictionary<string, int>();
        foreach (var item in cesta.Itens)
        {
            var valor = valorPorTicker[item.Ticker];
            var preco = precoPorTicker[item.Ticker];
            var qtdSemSaldo = preco > 0 ? (int)Math.Floor(valor / preco) : 0;
            var saldo = saldosMaster.GetValueOrDefault(item.Ticker, 0);
            quantidadeAComprar[item.Ticker] = Math.Max(0, qtdSemSaldo - saldo);
        }

        // 4) Detalhes lote padrão + fracionário (RN-031, RN-032, RN-033)
        var ordens = new List<OrdemCompraItem>();
        var compradoPorTicker = new Dictionary<string, int>();
        foreach (var item in cesta.Itens)
        {
            var qtd = quantidadeAComprar.GetValueOrDefault(item.Ticker, 0);
            var preco = precoPorTicker[item.Ticker];
            var (lotePadrao, fracionario) = SepararLoteFracionario(qtd);
            var detalhes = new List<DetalheOrdemDto>();
            if (lotePadrao > 0)
                detalhes.Add(new DetalheOrdemDto("LOTE_PADRAO", item.Ticker, lotePadrao));
            if (fracionario > 0)
                detalhes.Add(new DetalheOrdemDto("FRACIONARIO", item.Ticker + "F", fracionario));
            var totalQtd = lotePadrao + fracionario;
            ordens.Add(new OrdemCompraItem
            {
                Ticker = item.Ticker,
                QuantidadeTotal = totalQtd,
                PrecoUnitario = preco,
                ValorTotal = totalQtd * preco,
                Detalhes = detalhes
            });
            compradoPorTicker[item.Ticker] = totalQtd;
        }

        // 5) Disponível por ticker = saldo master + comprado
        var disponivelPorTicker = new Dictionary<string, int>();
        foreach (var t in tickers)
            disponivelPorTicker[t] = saldosMaster.GetValueOrDefault(t, 0) + compradoPorTicker.GetValueOrDefault(t, 0);

        // 6) Distribuição proporcional (RN-034 a RN-036): por cliente, TRUNCAR(proporção × disponível)
        var distribuicoes = new List<DistribuicaoCliente>();
        foreach (var (clienteId, nome, cpf, _, valorAporte) in aportes)
        {
            var proporcao = totalConsolidado > 0 ? valorAporte / totalConsolidado : 0;
            var ativos = new List<AtivoDistribuidoDto>();
            foreach (var item in cesta.Itens)
            {
                var disp = disponivelPorTicker.GetValueOrDefault(item.Ticker, 0);
                var qtdCliente = (int)Math.Floor(disp * proporcao);
                if (qtdCliente > 0)
                {
                    ativos.Add(new AtivoDistribuidoDto(item.Ticker, qtdCliente));
                    disponivelPorTicker[item.Ticker] = disp - qtdCliente;
                }
            }
            distribuicoes.Add(new DistribuicaoCliente
            {
                ClienteId = clienteId,
                Nome = nome,
                Cpf = cpf,
                ValorAporte = valorAporte,
                Ativos = ativos
            });
        }

        // 7) Persistir execução
        var execucao = new ExecucaoCompra
        {
            DataReferencia = dataReferencia,
            DataExecucao = DateTime.UtcNow,
            TotalConsolidado = totalConsolidado,
            TotalClientes = clientes.Count,
            Ordens = ordens,
            Distribuicoes = distribuicoes
        };
        await _execucaoRepo.SalvarExecucaoAsync(execucao, ct);

        // 8) Atualizar custódia master com resíduos (o que sobrou após distribuição por ticker)
        var residuos = tickers.Select(t => (t, disponivelPorTicker.GetValueOrDefault(t, 0))).ToList();
        await _custodiaRepo.DefinirResiduosAsync(residuos, ct);

        // 9) Registrar distribuição no serviço de Clientes (custódia filhote) e publicar IR dedo-duro
        foreach (var dist in distribuicoes)
        {
            var itensComPreco = dist.Ativos
                .Select(a => new ItemDistribuicaoDto(a.Ticker, a.Quantidade, precoPorTicker[a.Ticker]))
                .ToList();
            await _registroDistribuicaoClient.RegistrarDistribuicaoAsync(clienteId: dist.ClienteId, execucao.Id, itensComPreco, ct);

            foreach (var at in dist.Ativos)
            {
                var valorOp = at.Quantidade * precoPorTicker[at.Ticker];
                var valorIr = Math.Round(valorOp * AliquotaIrDedoDuro, 2);
                await _kafka.PublicarDedoDuroAsync(new EventoIRDedoDuro(
                    Tipo: "IR_DEDO_DURO",
                    ClienteId: dist.ClienteId,
                    Cpf: dist.Cpf,
                    Ticker: at.Ticker,
                    TipoOperacao: "COMPRA",
                    Quantidade: at.Quantidade,
                    PrecoUnitario: precoPorTicker[at.Ticker],
                    ValorOperacao: valorOp,
                    Aliquota: AliquotaIrDedoDuro,
                    ValorIR: valorIr,
                    DataOperacao: execucao.DataExecucao
                ), ct);
            }
        }

        return execucao;
    }

    private static (int LotePadrao, int Fracionario) SepararLoteFracionario(int quantidade)
    {
        var lotes = quantidade / 100;
        var frac = quantidade % 100;
        return (lotes * 100, frac);
    }
}
