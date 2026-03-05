using Microsoft.AspNetCore.Mvc;
using Rebalanceamento.Service.Application.Exceptions;
using Rebalanceamento.Service.Application.Ports;
using Rebalanceamento.Service.Application.Services;
using Rebalanceamento.Service.Controllers;
using Moq;
namespace CompraProgramada.Tests.Controllers;
public class RebalanceamentoControllerTests
{
    private readonly Mock<IExecutarRebalanceamentoService> _service;
    private readonly RebalanceamentoController _controller;
    public RebalanceamentoControllerTests()
    {
        _service = new Mock<IExecutarRebalanceamentoService>();
        _controller = new RebalanceamentoController(_service.Object);
    }
    [Fact]
    public async Task PorMudancaCesta_RequestNull_Retorna400()
    {
        var result = await _controller.PorMudancaCesta(null!, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task PorMudancaCesta_CestaAnteriorVazia_Retorna400()
    {
        var request = new PorMudancaCestaRequest(
            new List<ItemCestaRequest>(),
            new List<ItemCestaRequest> { new("PETR4", 20m) });
        var result = await _controller.PorMudancaCesta(request, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task PorMudancaCesta_CestaNovaVazia_Retorna400()
    {
        var request = new PorMudancaCestaRequest(
            new List<ItemCestaRequest> { new("PETR4", 20m) },
            new List<ItemCestaRequest>());
        var result = await _controller.PorMudancaCesta(request, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task PorMudancaCesta_ResultadoNull_Retorna400()
    {
        var request = new PorMudancaCestaRequest(
            new List<ItemCestaRequest> { new("PETR4", 20m) },
            new List<ItemCestaRequest> { new("PETR4", 25m), new("VALE3", 75m) });
        _service.Setup(s => s.ExecutarPorMudancaCestaAsync(It.IsAny<IReadOnlyList<ItemCestaDto>>(), It.IsAny<IReadOnlyList<ItemCestaDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResultadoRebalanceamentoDto?)null);
        var result = await _controller.PorMudancaCesta(request, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task PorMudancaCesta_Sucesso_Retorna200()
    {
        var request = new PorMudancaCestaRequest(
            new List<ItemCestaRequest> { new("PETR4", 20m), new("VALE3", 80m) },
            new List<ItemCestaRequest> { new("PETR4", 25m), new("VALE3", 75m) });
        var dto = new ResultadoRebalanceamentoDto(DateTime.UtcNow, 2, 1, 1, new List<string>());
        _service.Setup(s => s.ExecutarPorMudancaCestaAsync(It.IsAny<IReadOnlyList<ItemCestaDto>>(), It.IsAny<IReadOnlyList<ItemCestaDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        var result = await _controller.PorMudancaCesta(request, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task PorMudancaCesta_KafkaIndisponivel_Retorna500()
    {
        var request = new PorMudancaCestaRequest(
            new List<ItemCestaRequest> { new("PETR4", 20m) },
            new List<ItemCestaRequest> { new("PETR4", 25m) });
        _service.Setup(s => s.ExecutarPorMudancaCestaAsync(It.IsAny<IReadOnlyList<ItemCestaDto>>(), It.IsAny<IReadOnlyList<ItemCestaDto>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KafkaIndisponivelException());
        var result = await _controller.PorMudancaCesta(request, CancellationToken.None);
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }
    [Fact]
    public async Task PorDesvio_LimiarZero_Retorna400()
    {
        var result = await _controller.PorDesvio(0, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task PorDesvio_LimiarMaiorQue100_Retorna400()
    {
        var result = await _controller.PorDesvio(101, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task PorDesvio_ResultadoNull_Retorna400()
    {
        _service.Setup(s => s.ExecutarPorDesvioAsync(5m, It.IsAny<CancellationToken>())).ReturnsAsync((ResultadoRebalanceamentoDto?)null);
        var result = await _controller.PorDesvio(5m, CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task PorDesvio_Sucesso_Retorna200()
    {
        var dto = new ResultadoRebalanceamentoDto(DateTime.UtcNow, 2, 0, 0, new List<string>());
        _service.Setup(s => s.ExecutarPorDesvioAsync(5m, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.PorDesvio(5m, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task PorDesvio_KafkaIndisponivel_Retorna500()
    {
        _service.Setup(s => s.ExecutarPorDesvioAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ThrowsAsync(new KafkaIndisponivelException());
        var result = await _controller.PorDesvio(5m, CancellationToken.None);
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }
}
