using Microsoft.AspNetCore.Mvc;
using MotorCompra.Service.Application.Entities;
using MotorCompra.Service.Application.Exceptions;
using MotorCompra.Service.Application.Ports;
using MotorCompra.Service.Application.Services;
using MotorCompra.Service.Controllers;
using Moq;
namespace CompraProgramada.Tests.Controllers;
public class MotorControllerTests
{
    private readonly Mock<IExecutarCompraProgramadaService> _executarService;
    private readonly Mock<ICustodiaMasterRepository> _custodiaRepo;
    private readonly MotorController _controller;
    public MotorControllerTests()
    {
        _executarService = new Mock<IExecutarCompraProgramadaService>();
        _custodiaRepo = new Mock<ICustodiaMasterRepository>();
        _controller = new MotorController(_executarService.Object, _custodiaRepo.Object, null);
    }
    [Fact]
    public async Task ExecutarCompra_ComResultado_Retorna200()
    {
        var dataRef = new DateOnly(2026, 2, 5);
        var execucao = new ExecucaoCompra
        {
            Id = 1,
            DataReferencia = dataRef,
            DataExecucao = DateTime.UtcNow,
            TotalConsolidado = 3000m,
            TotalClientes = 2,
            Ordens = new List<OrdemCompraItem>(),
            Distribuicoes = new List<DistribuicaoCliente>()
        };
        var resultWithResiduos = new ExecucaoCompraComResiduos(execucao, new List<(string, int)>());
        _executarService.Setup(s => s.ExecutarAsync(dataRef, It.IsAny<CancellationToken>())).ReturnsAsync(resultWithResiduos);
        var result = await _controller.ExecutarCompra(new ExecutarCompraRequest(dataRef), CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        var dto = Assert.IsType<ExecucaoCompraResultDto>(ok.Value);
        Assert.Equal(2, dto.TotalClientes);
        Assert.Equal(3000m, dto.TotalConsolidado);
        Assert.NotNull(dto.ResiduosCustMaster);
        Assert.NotNull(dto.Mensagem);
    }
    [Fact]
    public async Task ExecutarCompra_SemResultado_Retorna204()
    {
        _executarService.Setup(s => s.ExecutarAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync((ExecucaoCompraComResiduos?)null);
        var result = await _controller.ExecutarCompra(new ExecutarCompraRequest(new DateOnly(2026, 2, 5)), CancellationToken.None);
        Assert.IsType<NoContentResult>(result.Result);
    }
    [Fact]
    public async Task ExecutarCompra_RequestNull_UsaDataHoje()
    {
        _executarService.Setup(s => s.ExecutarAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync((ExecucaoCompraComResiduos?)null);
        var result = await _controller.ExecutarCompra(null!, CancellationToken.None);
        _executarService.Verify(s => s.ExecutarAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result.Result);
    }
    [Fact]
    public async Task ExecutarCompra_CompraJaExecutada_Retorna409()
    {
        var dataRef = new DateOnly(2026, 2, 5);
        _executarService.Setup(s => s.ExecutarAsync(dataRef, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CompraJaExecutadaException(dataRef));
        var result = await _controller.ExecutarCompra(new ExecutarCompraRequest(dataRef), CancellationToken.None);
        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflict.StatusCode);
    }
    [Fact]
    public async Task ExecutarCompra_KafkaIndisponivel_Retorna500()
    {
        _executarService.Setup(s => s.ExecutarAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KafkaIndisponivelException());
        var result = await _controller.ExecutarCompra(new ExecutarCompraRequest(new DateOnly(2026, 2, 5)), CancellationToken.None);
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }
    [Fact]
    public async Task GetCustodiaMaster_SemResiduos_Retorna200ComCustodiaVazia()
    {
        _custodiaRepo.Setup(r => r.GetTodosResiduosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<(string Ticker, int Quantidade)>());
        var result = await _controller.GetCustodiaMaster(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CustodiaMasterResponseDto>(ok.Value);
        Assert.Empty(dto.Custodia);
        Assert.Equal(0, dto.ValorTotalResiduo);
    }
    [Fact]
    public async Task GetCustodiaMaster_ComResiduos_Retorna200ComItens()
    {
        var residuos = new List<(string Ticker, int Quantidade)> { ("PETR4", 10), ("VALE3", 5) };
        _custodiaRepo.Setup(r => r.GetTodosResiduosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(residuos);
        var result = await _controller.GetCustodiaMaster(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CustodiaMasterResponseDto>(ok.Value);
        Assert.Equal(2, dto.Custodia.Count);
        Assert.Equal("MST-000001", dto.ContaMaster.NumeroConta);
    }
}
