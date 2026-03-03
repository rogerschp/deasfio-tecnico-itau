using Cotacao.Application.Contracts;
using Cotacao.Application.DTOs;
using Cotacao.Application.Services;
using Cotacao.Domain;
using Moq;

namespace CompraProgramada.Tests.Cotacao;

public class CotacaoAppServiceTests
{
    private readonly Mock<ICotacaoRepository> _repositoryMock;
    private readonly Mock<ICotahistParser> _parserMock;
    private readonly CotacaoAppService _sut;

    public CotacaoAppServiceTests()
    {
        _repositoryMock = new Mock<ICotacaoRepository>();
        _parserMock = new Mock<ICotahistParser>();
        _sut = new CotacaoAppService(_repositoryMock.Object, _parserMock.Object);
    }

    [Fact]
    public async Task GetFechamentoAsync_TickerVazio_RetornaNull()
    {
        var result = await _sut.GetFechamentoAsync("");
        Assert.Null(result);

        result = await _sut.GetFechamentoAsync("   ");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFechamentoAsync_QuandoRepositorioRetornaNull_RetornaNull()
    {
        _repositoryMock
            .Setup(r => r.GetFechamentoUltimoPregaoAsync("PETR4", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CotacaoB3?)null);

        var result = await _sut.GetFechamentoAsync("PETR4");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFechamentoAsync_QuandoEncontrado_RetornaDto()
    {
        var entity = new CotacaoB3
        {
            Id = 1,
            DataPregao = new DateOnly(2026, 2, 25),
            Ticker = "PETR4",
            PrecoFechamento = 35.80m,
            PrecoAbertura = 35.20m,
            PrecoMaximo = 36.50m,
            PrecoMinimo = 34.80m
        };
        _repositoryMock
            .Setup(r => r.GetFechamentoUltimoPregaoAsync("PETR4", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetFechamentoAsync("PETR4");

        Assert.NotNull(result);
        Assert.Equal("PETR4", result.Ticker);
        Assert.Equal(new DateOnly(2026, 2, 25), result.DataPregao);
        Assert.Equal(35.80m, result.PrecoFechamento);
    }

    [Fact]
    public async Task GetFechamentoAsync_NormalizaTickerParaMaiusculas()
    {
        var entity = new CotacaoB3
        {
            Id = 1,
            DataPregao = new DateOnly(2026, 2, 25),
            Ticker = "PETR4",
            PrecoFechamento = 35.80m,
            PrecoAbertura = 0,
            PrecoMaximo = 0,
            PrecoMinimo = 0
        };
        _repositoryMock
            .Setup(r => r.GetFechamentoUltimoPregaoAsync("PETR4", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetFechamentoAsync("  petr4  ");

        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.GetFechamentoUltimoPregaoAsync("PETR4", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFechamentosAsync_ListaVaziaOuNull_RetornaListaVazia()
    {
        var empty = await _sut.GetFechamentosAsync(Array.Empty<string>());
        Assert.Empty(empty);

        var nullList = await _sut.GetFechamentosAsync(null!);
        Assert.Empty(nullList);
    }

    [Fact]
    public async Task GetFechamentosAsync_QuandoRepositorioRetorna_RetornaDtos()
    {
        var entities = new List<CotacaoB3>
        {
            new() { Ticker = "PETR4", DataPregao = new DateOnly(2026, 2, 25), PrecoFechamento = 35.80m },
            new() { Ticker = "VALE3", DataPregao = new DateOnly(2026, 2, 25), PrecoFechamento = 65.00m }
        };
        _repositoryMock
            .Setup(r => r.GetFechamentosUltimoPregaoPorTickersAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        var result = await _sut.GetFechamentosAsync(new[] { "PETR4", "VALE3" });

        Assert.Equal(2, result.Count);
        Assert.Equal("PETR4", result[0].Ticker);
        Assert.Equal(35.80m, result[0].PrecoFechamento);
        Assert.Equal("VALE3", result[1].Ticker);
        Assert.Equal(65.00m, result[1].PrecoFechamento);
    }

    [Fact]
    public async Task ImportarArquivoAsync_ArquivoInexistente_LancaFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _sut.ImportarArquivoAsync("/caminho/inexistente/COTAHIST.TXT"));
    }

    [Fact]
    public async Task ImportarArquivoAsync_ParserRetornaVazio_RetornaZeroInseridos()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "conteudo minimo");
            _parserMock
                .Setup(p => p.ParseFromFileAsync(tempFile, It.IsAny<CancellationToken>()))
                .Returns(AsyncEnumerableHelpers.ToAsyncEnumerable(Array.Empty<CotacaoB3>()));

            var result = await _sut.ImportarArquivoAsync(tempFile);

            Assert.Equal(0, result.RegistrosInseridos);
            Assert.False(result.PregaoJaExistia);
            _repositoryMock.Verify(r => r.BulkInsertAsync(It.IsAny<IEnumerable<CotacaoB3>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportarArquivoAsync_PregaoJaExistia_NaoInsere()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "x");
            var cotacao = new CotacaoB3 { DataPregao = new DateOnly(2026, 2, 25), Ticker = "PETR4", PrecoFechamento = 35.80m };
            _parserMock
                .Setup(p => p.ParseFromFileAsync(tempFile, It.IsAny<CancellationToken>()))
                .Returns(AsyncEnumerableHelpers.ToAsyncEnumerable(new[] { cotacao }));
            _repositoryMock
                .Setup(r => r.ExistePregaoAsync(new DateOnly(2026, 2, 25), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.ImportarArquivoAsync(tempFile);

            Assert.True(result.PregaoJaExistia);
            Assert.Equal(0, result.RegistrosInseridos);
            _repositoryMock.Verify(r => r.BulkInsertAsync(It.IsAny<IEnumerable<CotacaoB3>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportarArquivoAsync_ComDados_ChamaBulkInsert()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "x");
            var cotacao = new CotacaoB3 { DataPregao = new DateOnly(2026, 2, 25), Ticker = "PETR4", PrecoFechamento = 35.80m };
            _parserMock
                .Setup(p => p.ParseFromFileAsync(tempFile, It.IsAny<CancellationToken>()))
                .Returns(AsyncEnumerableHelpers.ToAsyncEnumerable(new[] { cotacao }));
            _repositoryMock
                .Setup(r => r.ExistePregaoAsync(new DateOnly(2026, 2, 25), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _repositoryMock
                .Setup(r => r.BulkInsertAsync(It.IsAny<IEnumerable<CotacaoB3>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _sut.ImportarArquivoAsync(tempFile);

            Assert.False(result.PregaoJaExistia);
            Assert.Equal(1, result.RegistrosInseridos);
            Assert.Equal(new DateOnly(2026, 2, 25), result.DataPregao);
            _repositoryMock.Verify(r => r.BulkInsertAsync(It.IsAny<IEnumerable<CotacaoB3>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
