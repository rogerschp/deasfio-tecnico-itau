using Moq;
using Rebalanceamento.Service.Application.Ports;
using Rebalanceamento.Service.Application.Services;
using Shared.Kafka;
namespace CompraProgramada.Tests.Rebalanceamento;
public class ExecutarRebalanceamentoServiceTests
{
    private readonly Mock<IClientesRebalanceamentoClient> _clientesMock;
    private readonly Mock<ICestaVigenteClient> _cestaMock;
    private readonly Mock<ICotacaoFechamentoClient> _cotacaoMock;
    private readonly Mock<IVendaRebalanceamentoRepository> _vendaRepoMock;
    private readonly Mock<IEventoIRPublisher> _kafkaMock;
    private readonly ExecutarRebalanceamentoService _sut;
    public ExecutarRebalanceamentoServiceTests()
    {
        _clientesMock = new Mock<IClientesRebalanceamentoClient>();
        _cestaMock = new Mock<ICestaVigenteClient>();
        _cotacaoMock = new Mock<ICotacaoFechamentoClient>();
        _vendaRepoMock = new Mock<IVendaRebalanceamentoRepository>();
        _kafkaMock = new Mock<IEventoIRPublisher>();
        _sut = new ExecutarRebalanceamentoService(
            _clientesMock.Object,
            _cestaMock.Object,
            _cotacaoMock.Object,
            _vendaRepoMock.Object,
            _kafkaMock.Object);
    }
    private static IReadOnlyList<ItemCestaDto> CestaAntiga => new List<ItemCestaDto>
    {
        new("PETR4", 30m),
        new("VALE3", 25m),
        new("ITUB4", 20m),
        new("BBDC4", 15m),
        new("WEGE3", 10m)
    };
    private static IReadOnlyList<ItemCestaDto> CestaNova => new List<ItemCestaDto>
    {
        new("PETR4", 25m),
        new("VALE3", 20m),
        new("ITUB4", 20m),
        new("ABEV3", 20m),
        new("RENT3", 15m)
    };
    [Fact]
    public async Task ExecutarPorMudancaCestaAsync_CestaAnteriorOuNovaNula_RetornaNull()
    {
        var result = await _sut.ExecutarPorMudancaCestaAsync(null!, CestaNova.ToList(), default);
        Assert.Null(result);
        result = await _sut.ExecutarPorMudancaCestaAsync(CestaAntiga.ToList(), null!, default);
        Assert.Null(result);
    }
    [Fact]
    public async Task ExecutarPorMudancaCestaAsync_SemClientesAtivos_RetornaResultadoComZeroClientes()
    {
        _clientesMock.Setup(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ClienteAtivoDto>());
        var result = await _sut.ExecutarPorMudancaCestaAsync(CestaAntiga.ToList(), CestaNova.ToList(), default);
        Assert.NotNull(result);
        Assert.Equal(0, result.ClientesProcessados);
    }
    [Fact]
    public async Task ExecutarPorMudancaCestaAsync_CotacaoIndisponivel_RetornaResultadoComErro()
    {
        _clientesMock.Setup(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClienteAtivoDto> { new(1, "A", "111", 1000m, 10) });
        _cotacaoMock.Setup(c => c.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CotacaoFechamentoDto>());
        var result = await _sut.ExecutarPorMudancaCestaAsync(CestaAntiga.ToList(), CestaNova.ToList(), default);
        Assert.NotNull(result);
        Assert.Contains(result.Erros, e => e.Contains("Cotação"));
    }
    [Fact]
    public async Task ExecutarPorDesvioAsync_SemCestaVigente_RetornaResultadoComErro()
    {
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync((CestaVigenteDto?)null);
        var result = await _sut.ExecutarPorDesvioAsync(5m, default);
        Assert.NotNull(result);
        Assert.Contains(result.Erros, e => e.Contains("Cesta"));
    }
    [Fact]
    public async Task ExecutarPorDesvioAsync_SemClientesAtivos_RetornaResultadoComZeroClientes()
    {
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CestaVigenteDto(1, "Top", CestaAntiga.ToList()));
        _clientesMock.Setup(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ClienteAtivoDto>());
        var result = await _sut.ExecutarPorDesvioAsync(5m, default);
        Assert.NotNull(result);
        Assert.Equal(0, result.ClientesProcessados);
    }
    [Fact]
    public async Task ExecutarPorDesvioAsync_VendasAbaixoDe20k_NaoPublicaIRVenda()
    {
        _cestaMock.Setup(c => c.GetCestaVigenteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CestaVigenteDto(1, "Top", CestaAntiga.ToList()));
        _clientesMock.Setup(c => c.GetClientesAtivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClienteAtivoDto> { new(1, "A", "111", 1000m, 10) });
        _clientesMock.Setup(c => c.GetCarteiraAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CarteiraClienteDto(1, new List<PosicaoCustodiaDto> { new("PETR4", 10, 35m) }));
        _cotacaoMock.Setup(c => c.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CotacaoFechamentoDto> { new("PETR4", 36m) });
        _vendaRepoMock.Setup(r => r.GetTotalVendasELucroClienteNoMesAsync(1, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((1000m, 50m));
        await _sut.ExecutarPorDesvioAsync(5m, default);
        _kafkaMock.Verify(k => k.PublicarIRVendaAsync(It.IsAny<Shared.Contracts.Eventos.EventoIRVenda>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
