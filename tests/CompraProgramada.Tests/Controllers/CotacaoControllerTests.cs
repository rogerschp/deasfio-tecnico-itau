using Cotacao.Application.DTOs;
using Cotacao.Application.Services;
using Cotacao.Service.Controllers;
using Cotacao.Service.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
namespace CompraProgramada.Tests.Controllers;
public class CotacaoControllerTests
{
    private readonly Mock<ICotacaoAppService> _cotacaoService;
    private readonly CotacaoController _controller;
    public CotacaoControllerTests()
    {
        _cotacaoService = new Mock<ICotacaoAppService>();
        var options = Options.Create(new CotacaoServiceOptions { PastaCotacoes = "cotacoes" });
        _controller = new CotacaoController(_cotacaoService.Object, options);
    }
    [Fact]
    public async Task GetFechamento_Encontrado_Retorna200()
    {
        var dto = new CotacaoFechamentoDto("PETR4", new DateOnly(2026, 2, 25), 35.50m);
        _cotacaoService.Setup(s => s.GetFechamentoAsync("PETR4", It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.GetFechamento("PETR4", CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task GetFechamento_NaoEncontrado_Retorna404()
    {
        _cotacaoService.Setup(s => s.GetFechamentoAsync("INVALID", It.IsAny<CancellationToken>())).ReturnsAsync((CotacaoFechamentoDto?)null);
        var result = await _controller.GetFechamento("INVALID", CancellationToken.None);
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task GetFechamentos_TickersVazios_Retorna200ListaVazia()
    {
        _cotacaoService.Setup(s => s.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CotacaoFechamentoDto>());
        var result = await _controller.GetFechamentos(null, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IReadOnlyList<CotacaoFechamentoDto>>(ok.Value);
        Assert.Empty(list);
    }
    [Fact]
    public async Task GetFechamentos_ComTickers_Retorna200ComLista()
    {
        var list = new List<CotacaoFechamentoDto>
        {
            new("PETR4", new DateOnly(2026, 2, 25), 35.50m),
            new("VALE3", new DateOnly(2026, 2, 25), 62.00m)
        };
        _cotacaoService.Setup(s => s.GetFechamentosAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);
        var result = await _controller.GetFechamentos("PETR4,VALE3", CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var resultList = Assert.IsAssignableFrom<IReadOnlyList<CotacaoFechamentoDto>>(ok.Value);
        Assert.Equal(2, resultList.Count);
    }
    [Fact]
    public async Task Importar_CaminhoVazio_Retorna400()
    {
        var result = await _controller.Importar(new ImportarCotahistRequest(""), CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task Importar_RequestNull_Retorna400()
    {
        var result = await _controller.Importar(null!, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task Importar_Sucesso_Retorna200()
    {
        var dto = new ImportacaoResultDto(new DateOnly(2026, 2, 25), 150, false);
        _cotacaoService.Setup(s => s.ImportarArquivoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.Importar(new ImportarCotahistRequest("/pasta/cotacoes/COTA.txt"), CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task Importar_ArquivoNaoEncontrado_Retorna404()
    {
        _cotacaoService.Setup(s => s.ImportarArquivoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Arquivo não encontrado."));
        var result = await _controller.Importar(new ImportarCotahistRequest("/inexistente.txt"), CancellationToken.None);
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
