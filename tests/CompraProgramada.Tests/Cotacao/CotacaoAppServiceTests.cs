using Cotacao.Application.Contracts;
using Cotacao.Application.DTOs;
using Cotacao.Application.Services;
using Cotacao.Domain;
using Moq;
namespace CompraProgramada.Tests.Cotacao;
public class CotacaoAppServiceTests
{
    private readonly Mock<ICotacaoRepository> _repository;
    private readonly Mock<ICotahistParser> _parser;
    private readonly CotacaoAppService _sut;
    public CotacaoAppServiceTests()
    {
        _repository = new Mock<ICotacaoRepository>();
        _parser = new Mock<ICotahistParser>();
        _sut = new CotacaoAppService(_repository.Object, _parser.Object);
    }
    [Fact]
    public async Task GetFechamentoAsync_TickerVazio_RetornaNull()
    {
        var result = await _sut.GetFechamentoAsync("");
        Assert.Null(result);
    }
    [Fact]
    public async Task GetFechamentoAsync_TickerNaoEncontrado_RetornaNull()
    {
        _repository.Setup(r => r.GetFechamentoUltimoPregaoAsync("INVALID", It.IsAny<CancellationToken>())).ReturnsAsync((CotacaoB3?)null);
        var result = await _sut.GetFechamentoAsync("INVALID");
        Assert.Null(result);
    }
    [Fact]
    public async Task GetFechamentoAsync_Encontrado_RetornaDto()
    {
        var entity = new CotacaoB3 { Ticker = "PETR4", DataPregao = new DateOnly(2026, 2, 25), PrecoFechamento = 35.50m };
        _repository.Setup(r => r.GetFechamentoUltimoPregaoAsync("PETR4", It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var result = await _sut.GetFechamentoAsync("PETR4");
        Assert.NotNull(result);
        Assert.Equal("PETR4", result.Ticker);
        Assert.Equal(35.50m, result.PrecoFechamento);
    }
    [Fact]
    public async Task GetFechamentosAsync_TickersNull_RetornaListaVazia()
    {
        var result = await _sut.GetFechamentosAsync(null!);
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    [Fact]
    public async Task GetFechamentosAsync_TickersVazios_RetornaListaVazia()
    {
        var result = await _sut.GetFechamentosAsync(new List<string>());
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    [Fact]
    public async Task GetFechamentosAsync_ComTickers_RetornaLista()
    {
        var entities = new List<CotacaoB3>
        {
            new() { Ticker = "PETR4", DataPregao = new DateOnly(2026, 2, 25), PrecoFechamento = 35.50m },
            new() { Ticker = "VALE3", DataPregao = new DateOnly(2026, 2, 25), PrecoFechamento = 62m }
        };
        _repository.Setup(r => r.GetFechamentosUltimoPregaoPorTickersAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(entities);
        var result = await _sut.GetFechamentosAsync(new List<string> { "PETR4", "VALE3" });
        Assert.Equal(2, result.Count);
        Assert.Equal("PETR4", result[0].Ticker);
        Assert.Equal("VALE3", result[1].Ticker);
    }
    [Fact]
    public async Task ImportarArquivoAsync_CaminhoVazio_LancaFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => _sut.ImportarArquivoAsync(""));
    }
    [Fact]
    public async Task ImportarArquivoAsync_ArquivoNaoExiste_LancaFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => _sut.ImportarArquivoAsync("/caminho/inexistente.txt"));
    }
    [Fact]
    public async Task ImportarArquivoAsync_ArquivoVazio_RetornaZeroInseridos()
    {
        var path = Path.GetTempFileName();
        try
        {
            _parser.Setup(p => p.ParseFromFileAsync(path, It.IsAny<CancellationToken>())).Returns(EmptyAsyncEnumerable());
            var result = await _sut.ImportarArquivoAsync(path);
            Assert.Equal(0, result.RegistrosInseridos);
            Assert.False(result.PregaoJaExistia);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
    [Fact]
    public async Task ImportarArquivoAsync_PregaoJaExistia_RetornaPregaoJaExistiaTrue()
    {
        var path = Path.GetTempFileName();
        try
        {
            var data = new DateOnly(2026, 2, 25);
            _parser.Setup(p => p.ParseFromFileAsync(path, It.IsAny<CancellationToken>())).Returns(OneItemAsync(data));
            _repository.Setup(r => r.ExistePregaoAsync(data, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = await _sut.ImportarArquivoAsync(path);
            Assert.Equal(0, result.RegistrosInseridos);
            Assert.True(result.PregaoJaExistia);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
    [Fact]
    public async Task ImportarArquivoAsync_NovoPregao_InsereERetornaQuantidade()
    {
        var path = Path.GetTempFileName();
        try
        {
            var data = new DateOnly(2026, 2, 25);
            _parser.Setup(p => p.ParseFromFileAsync(path, It.IsAny<CancellationToken>())).Returns(OneItemAsync(data));
            _repository.Setup(r => r.ExistePregaoAsync(data, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _repository.Setup(r => r.BulkInsertAsync(It.IsAny<IEnumerable<CotacaoB3>>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var result = await _sut.ImportarArquivoAsync(path);
            Assert.Equal(1, result.RegistrosInseridos);
            Assert.False(result.PregaoJaExistia);
            Assert.Equal(data, result.DataPregao);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
    private static async IAsyncEnumerable<CotacaoB3> EmptyAsyncEnumerable()
    {
        await Task.CompletedTask;
        yield break;
    }
    private static async IAsyncEnumerable<CotacaoB3> OneItemAsync(DateOnly data)
    {
        await Task.CompletedTask;
        yield return new CotacaoB3 { Ticker = "PETR4", DataPregao = data, PrecoFechamento = 35m };
    }
}
