using Admin.Service.Application.DTOs;
using Admin.Service.Application.Entities;
using Admin.Service.Application.Ports;
using Admin.Service.Application.Services;
using Moq;
namespace CompraProgramada.Tests.Admin;
public class CestaAppServiceTests
{
    private readonly Mock<ICestaRepository> _repositoryMock;
    private readonly CestaAppService _sut;
    public CestaAppServiceTests()
    {
        _repositoryMock = new Mock<ICestaRepository>();
        _sut = new CestaAppService(_repositoryMock.Object);
    }
    private static CestaAppService CreateWithCotacaoClient(Mock<ICestaRepository> repo, Mock<ICotacaoFechamentoClient>? cotacaoClient)
    {
        return new CestaAppService(repo.Object, cotacaoClient?.Object);
    }
    private static IReadOnlyList<(string Ticker, decimal Percentual)> ItensValidos => new List<(string, decimal)>
    {
        ("PETR4", 30m),
        ("VALE3", 25m),
        ("ITUB4", 20m),
        ("BBDC4", 15m),
        ("WEGE3", 10m)
    };
    [Fact]
    public async Task CadastrarOuAlterarAsync_QuantidadeDiferenteDeCinco_LancaArgumentException()
    {
        var itens = new List<(string, decimal)> { ("PETR4", 100m) };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CadastrarOuAlterarAsync("Cesta", itens));
    }
    [Fact]
    public async Task CadastrarOuAlterarAsync_SomaPercentualDiferenteDe100_LancaArgumentException()
    {
        var itens = new List<(string, decimal)>
        {
            ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("WEGE3", 15m)
        };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CadastrarOuAlterarAsync("Cesta", itens));
    }
    [Fact]
    public async Task CadastrarOuAlterarAsync_PercentualZero_LancaArgumentException()
    {
        var itens = new List<(string, decimal)>
        {
            ("PETR4", 30m), ("VALE3", 25m), ("ITUB4", 20m), ("BBDC4", 15m), ("WEGE3", 0m)
        };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CadastrarOuAlterarAsync("Cesta", itens));
    }
    [Fact]
    public async Task CadastrarOuAlterarAsync_SemCestaAnterior_DesativaNuncaChamado()
    {
        _repositoryMock.Setup(r => r.GetAtivaAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Cesta?)null);
        var cestaSalva = new Cesta { Id = 1, Nome = "Top Five", Ativa = true, DataCriacao = DateTime.UtcNow, Itens = ItensValidos.Select(i => new ItemCesta { Ticker = i.Ticker, Percentual = i.Percentual }).ToList() };
        _repositoryMock.Setup(r => r.SalvarAsync(It.IsAny<Cesta>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cesta c, CancellationToken _) => { c.Id = 1; return c; });
        var result = await _sut.CadastrarOuAlterarAsync("Top Five", ItensValidos.ToList());
        Assert.NotNull(result);
        Assert.Equal(1, result.CestaId);
        Assert.True(result.Ativa);
        Assert.False(result.RebalanceamentoDisparado);
        Assert.Null(result.CestaAnteriorDesativada);
        _repositoryMock.Verify(r => r.DesativarAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task CadastrarOuAlterarAsync_ComCestaAnterior_DesativaERetornaRebalanceamento()
    {
        var anterior = new Cesta
        {
            Id = 1,
            Nome = "Antiga",
            Ativa = true,
            DataCriacao = DateTime.UtcNow.AddDays(-10),
            Itens = ItensValidos.Select(i => new ItemCesta { Ticker = i.Ticker, Percentual = i.Percentual }).ToList()
        };
        _repositoryMock.Setup(r => r.GetAtivaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(anterior);
        _repositoryMock.Setup(r => r.SalvarAsync(It.IsAny<Cesta>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cesta c, CancellationToken _) => { c.Id = 2; return c; });
        var result = await _sut.CadastrarOuAlterarAsync("Nova Cesta", ItensValidos.ToList());
        Assert.True(result.RebalanceamentoDisparado);
        Assert.NotNull(result.CestaAnteriorDesativada);
        Assert.Equal(1, result.CestaAnteriorDesativada.CestaId);
        _repositoryMock.Verify(r => r.DesativarAsync(1, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task GetAtualAsync_QuandoRepositorioRetornaNull_RetornaNull()
    {
        _repositoryMock.Setup(r => r.GetAtivaAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Cesta?)null);
        var result = await _sut.GetAtualAsync();
        Assert.Null(result);
    }
    [Fact]
    public async Task GetAtualAsync_QuandoEncontrado_RetornaDto()
    {
        var cesta = new Cesta
        {
            Id = 1,
            Nome = "Top Five",
            Ativa = true,
            DataCriacao = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc),
            Itens = ItensValidos.Select(i => new ItemCesta { Ticker = i.Ticker, Percentual = i.Percentual }).ToList()
        };
        _repositoryMock.Setup(r => r.GetAtivaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(cesta);
        var result = await _sut.GetAtualAsync();
        Assert.NotNull(result);
        Assert.Equal(1, result.CestaId);
        Assert.Equal("Top Five", result.Nome);
        Assert.Equal(5, result.Itens.Count);
        Assert.Equal("PETR4", result.Itens[0].Ticker);
        Assert.Equal(30m, result.Itens[0].Percentual);
    }
    [Fact]
    public async Task GetHistoricoAsync_RetornaListaDoRepositorio()
    {
        var list = new List<Cesta>
        {
            new() { Id = 1, Nome = "C1", Ativa = false, DataCriacao = DateTime.UtcNow, DataDesativacao = DateTime.UtcNow, Itens = new List<ItemCesta>() }
        };
        _repositoryMock.Setup(r => r.GetHistoricoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        var result = await _sut.GetHistoricoAsync();
        Assert.Single(result.Cestas);
        Assert.Equal(1, result.Cestas[0].CestaId);
        Assert.Equal("C1", result.Cestas[0].Nome);
        Assert.False(result.Cestas[0].Ativa);
    }
    [Fact]
    public async Task GetAtualAsync_ComCotacaoClient_PreencheCotacaoAtualNosItens()
    {
        var cotacaoClient = new Mock<ICotacaoFechamentoClient>();
        cotacaoClient.Setup(c => c.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CotacaoFechamentoDto> { new("PETR4", 35.50m), new("VALE3", 62m) });
        var cesta = new Cesta
        {
            Id = 1,
            Nome = "Top Five",
            Ativa = true,
            DataCriacao = DateTime.UtcNow,
            Itens = new List<ItemCesta> { new() { Ticker = "PETR4", Percentual = 50m }, new() { Ticker = "VALE3", Percentual = 50m } }
        };
        _repositoryMock.Setup(r => r.GetAtivaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(cesta);
        var sutComCotacao = CreateWithCotacaoClient(_repositoryMock, cotacaoClient);
        var result = await sutComCotacao.GetAtualAsync();
        Assert.NotNull(result);
        Assert.Equal(2, result.Itens.Count);
        Assert.Equal(35.50m, result.Itens[0].CotacaoAtual);
        Assert.Equal(62m, result.Itens[1].CotacaoAtual);
    }
}
