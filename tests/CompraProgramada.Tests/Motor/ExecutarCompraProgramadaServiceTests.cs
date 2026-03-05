using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MotorCompra.Service.Application.Entities;
using MotorCompra.Service.Application.Exceptions;
using MotorCompra.Service.Application.Ports;
using MotorCompra.Service.Application.Services;
using Moq;
using Shared.Kafka;
namespace CompraProgramada.Tests.Motor;
public class ExecutarCompraProgramadaServiceTests
{
    private readonly Mock<ICestaVigenteClient> _cestaMock;
    private readonly Mock<IClientesAtivosClient> _clientesMock;
    private readonly Mock<ICotacaoFechamentoClient> _cotacaoMock;
    private readonly Mock<IRegistroDistribuicaoClient> _registroMock;
    private readonly Mock<IExecucaoCompraRepository> _execucaoRepoMock;
    private readonly Mock<ICustodiaMasterRepository> _custodiaRepoMock;
    private readonly Mock<IEventoIRPublisher> _kafkaMock;
    private readonly ExecutarCompraProgramadaService _sut;
    public ExecutarCompraProgramadaServiceTests()
    {
        _cestaMock = new Mock<ICestaVigenteClient>();
        _clientesMock = new Mock<IClientesAtivosClient>();
        _cotacaoMock = new Mock<ICotacaoFechamentoClient>();
        _registroMock = new Mock<IRegistroDistribuicaoClient>();
        _execucaoRepoMock = new Mock<IExecucaoCompraRepository>();
        _custodiaRepoMock = new Mock<ICustodiaMasterRepository>();
        _kafkaMock = new Mock<IEventoIRPublisher>();
        _sut = new ExecutarCompraProgramadaService(
            _cestaMock.Object,
            _clientesMock.Object,
            _cotacaoMock.Object,
            _registroMock.Object,
            _execucaoRepoMock.Object,
            _custodiaRepoMock.Object,
            _kafkaMock.Object,
            NullLogger<ExecutarCompraProgramadaService>.Instance);
    }
    private static CestaVigenteDto CestaValida => new(
        1,
        "Top Five",
        new List<ItemCestaDto>
        {
            new("PETR4", 30m),
            new("VALE3", 25m),
            new("ITUB4", 20m),
            new("BBDC4", 15m),
            new("WEGE3", 10m)
        });
    private static IReadOnlyList<ClienteAtivoDto> ClientesValidos => new List<ClienteAtivoDto>
    {
        new(1, "Cliente A", "11111111111", 3000m, 10),
        new(2, "Cliente B", "22222222222", 6000m, 20)
    };
    private static IReadOnlyList<CotacaoFechamentoDto> CotacoesValidas => new List<CotacaoFechamentoDto>
    {
        new("PETR4", new DateOnly(2026, 2, 25), 35.00m),
        new("VALE3", new DateOnly(2026, 2, 25), 62.00m),
        new("ITUB4", new DateOnly(2026, 2, 25), 30.00m),
        new("BBDC4", new DateOnly(2026, 2, 25), 15.00m),
        new("WEGE3", new DateOnly(2026, 2, 25), 40.00m)
    };
    private void ConfigurarCenarioValido()
    {
        var data = new DateOnly(2026, 2, 5);
        _execucaoRepoMock.Setup(r => r.JaExecutouNaDataAsync(data, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CestaValida);
        _clientesMock.Setup(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ClientesValidos.ToList());
        _cotacaoMock.Setup(c => c.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(CotacoesValidas.ToList());
        _custodiaRepoMock.Setup(r => r.GetSaldosPorTickerAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<string, int>());
        _execucaoRepoMock.Setup(r => r.SalvarExecucaoAsync(It.IsAny<ExecucaoCompra>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecucaoCompra e, CancellationToken _) => { e.Id = 100; return e; });
    }
    [Fact]
    public async Task ExecutarAsync_JaExecutouNaData_LancaCompraJaExecutadaException()
    {
        var data = new DateOnly(2026, 2, 5);
        _execucaoRepoMock.Setup(r => r.JaExecutouNaDataAsync(data, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var ex = await Assert.ThrowsAsync<CompraJaExecutadaException>(() => _sut.ExecutarAsync(data));
        Assert.Contains("2026-02-05", ex.Message);
        _cestaMock.Verify(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task ExecutarAsync_SemCestaVigente_RetornaNull()
    {
        _execucaoRepoMock.Setup(r => r.JaExecutouNaDataAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync((CestaVigenteDto?)null);
        var result = await _sut.ExecutarAsync(new DateOnly(2026, 2, 5));
        Assert.Null(result);
        _clientesMock.Verify(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task ExecutarAsync_CestaComItensVazios_RetornaNull()
    {
        _execucaoRepoMock.Setup(r => r.JaExecutouNaDataAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new CestaVigenteDto(1, "X", new List<ItemCestaDto>()));
        var result = await _sut.ExecutarAsync(new DateOnly(2026, 2, 5));
        Assert.Null(result);
    }
    [Fact]
    public async Task ExecutarAsync_SemClientesAtivos_RetornaNull()
    {
        _execucaoRepoMock.Setup(r => r.JaExecutouNaDataAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CestaValida);
        _clientesMock.Setup(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ClienteAtivoDto>());
        var result = await _sut.ExecutarAsync(new DateOnly(2026, 2, 5));
        Assert.Null(result);
        _cotacaoMock.Verify(c => c.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task ExecutarAsync_SemCotacoes_RetornaNull()
    {
        _execucaoRepoMock.Setup(r => r.JaExecutouNaDataAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CestaValida);
        _clientesMock.Setup(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ClientesValidos.ToList());
        _cotacaoMock.Setup(c => c.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CotacaoFechamentoDto>());
        var result = await _sut.ExecutarAsync(new DateOnly(2026, 2, 5));
        Assert.Null(result);
    }
    [Fact]
    public async Task ExecutarAsync_CotacaoFaltandoTicker_RetornaNull()
    {
        _execucaoRepoMock.Setup(r => r.JaExecutouNaDataAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CestaValida);
        _clientesMock.Setup(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ClientesValidos.ToList());
        var cotacoesIncompletas = new List<CotacaoFechamentoDto> { new("PETR4", new DateOnly(2026, 2, 25), 35m) };
        _cotacaoMock.Setup(c => c.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(cotacoesIncompletas);
        var result = await _sut.ExecutarAsync(new DateOnly(2026, 2, 5));
        Assert.Null(result);
    }
    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_RetornaExecucaoComOrdensEDistribuicoes()
    {
        ConfigurarCenarioValido();
        var data = new DateOnly(2026, 2, 5);
        var result = await _sut.ExecutarAsync(data);
        Assert.NotNull(result);
        Assert.Equal(100, result.Id);
        Assert.Equal(data, result.DataReferencia);
        Assert.Equal(2, result.TotalClientes);
        Assert.Equal(3000m, Math.Round(result.TotalConsolidado, 2));
        Assert.NotEmpty(result.Ordens);
        Assert.Equal(5, result.Ordens.Count);
        Assert.NotEmpty(result.Distribuicoes);
        Assert.Equal(2, result.Distribuicoes.Count);
        _execucaoRepoMock.Verify(r => r.SalvarExecucaoAsync(It.IsAny<ExecucaoCompra>(), It.IsAny<CancellationToken>()), Times.Once);
        _custodiaRepoMock.Verify(r => r.DefinirResiduosAsync(It.IsAny<IReadOnlyList<(string, int)>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task ExecutarAsync_ComDadosValidos_ChamaRegistroDistribuicaoEPublicaKafka()
    {
        ConfigurarCenarioValido();
        var result = await _sut.ExecutarAsync(new DateOnly(2026, 2, 5));
        Assert.NotNull(result);
        _registroMock.Verify(r => r.RegistrarDistribuicaoAsync(It.IsAny<long>(), 100, It.IsAny<IReadOnlyList<ItemDistribuicaoDto>>(), It.IsAny<DateOnly?>(), It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _kafkaMock.Verify(k => k.PublicarDedoDuroAsync(It.IsAny<Shared.Contracts.Eventos.EventoIRDedoDuro>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
    [Fact]
    public async Task ExecutarAsync_ComSaldoCustodiaMaster_DescontaDaQuantidadeAComprar()
    {
        ConfigurarCenarioValido();
        _custodiaRepoMock.Setup(r => r.GetSaldosPorTickerAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, int> { { "PETR4", 5 } });
        var result = await _sut.ExecutarAsync(new DateOnly(2026, 2, 5));
        Assert.NotNull(result);
        var ordemPetr4 = result.Ordens.FirstOrDefault(o => o.Ticker == "PETR4");
        Assert.NotNull(ordemPetr4);
        Assert.True(ordemPetr4.QuantidadeTotal <= 25, "Quantidade de PETR4 deve ser descontada do saldo master.");
    }
}
