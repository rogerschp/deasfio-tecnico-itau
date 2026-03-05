using Admin.Service.Application.DTOs;
using Admin.Service.Application.Ports;
using Admin.Service.Application.Services;
using Admin.Service.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Contracts.Admin;
using CustodiaMasterResponseDto = Admin.Service.Application.Ports.CustodiaMasterResponseDto;
using ContaMasterDto = Admin.Service.Application.Ports.ContaMasterDto;
using CustodiaMasterItemDto = Admin.Service.Application.Ports.CustodiaMasterItemDto;
namespace CompraProgramada.Tests.Controllers;
public class AdminControllerTests
{
    private readonly Mock<ICestaAppService> _cestaService;
    private readonly Mock<ICustodiaMasterClient> _custodiaMasterClient;
    private readonly AdminController _controller;
    public AdminControllerTests()
    {
        _cestaService = new Mock<ICestaAppService>();
        _custodiaMasterClient = new Mock<ICustodiaMasterClient>();
        _controller = new AdminController(_cestaService.Object, _custodiaMasterClient.Object);
    }
    [Fact]
    public async Task CadastrarCesta_NomeVazio_RetornaBadRequest()
    {
        var result = await _controller.CadastrarCesta(new CestaRequest("", new List<ItemCestaRequest>()));
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task CadastrarCesta_ItensVazios_RetornaBadRequest()
    {
        var result = await _controller.CadastrarCesta(new CestaRequest("Top Five", new List<ItemCestaRequest>()));
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task CadastrarCesta_Sucesso_Retorna201()
    {
        var itens = new List<ItemCestaRequest>
        {
            new("PETR4", 20m), new("VALE3", 20m), new("ITUB4", 20m), new("BBDC4", 20m), new("WEGE3", 20m)
        };
        var dto = new CestaResponseDto(1, "Top Five", true, DateTime.UtcNow, new List<ItemCestaDto>());
        _cestaService.Setup(s => s.CadastrarOuAlterarAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, decimal)>>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.CadastrarCesta(new CestaRequest("Top Five", itens));
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Same(dto, created.Value);
    }
    [Fact]
    public async Task CadastrarCesta_ServicoLancaPercentuaisInvalidos_Retorna400()
    {
        var itens = new List<ItemCestaRequest>
        {
            new("PETR4", 20m), new("VALE3", 20m), new("ITUB4", 20m), new("BBDC4", 20m), new("WEGE3", 15m)
        };
        _cestaService.Setup(s => s.CadastrarOuAlterarAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, decimal)>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("A soma dos percentuais deve ser exatamente 100%."));
        var result = await _controller.CadastrarCesta(new CestaRequest("Top Five", itens));
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task CadastrarCesta_ServicoLancaQuantidadeAtivosInvalida_Retorna400()
    {
        var itens = new List<ItemCestaRequest> { new("PETR4", 25m), new("VALE3", 25m), new("ITUB4", 25m), new("WEGE3", 25m) };
        _cestaService.Setup(s => s.CadastrarOuAlterarAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<(string, decimal)>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("A cesta deve conter exatamente 5 ativos. Quantidade informada: 4."));
        var result = await _controller.CadastrarCesta(new CestaRequest("Top Five", itens));
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task GetCestaAtual_NenhumaCesta_Retorna404()
    {
        _cestaService.Setup(s => s.GetAtualAsync(It.IsAny<CancellationToken>())).ReturnsAsync((CestaResponseDto?)null);
        var result = await _controller.GetCestaAtual();
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task GetCestaAtual_Sucesso_Retorna200()
    {
        var dto = new CestaResponseDto(1, "Top Five", true, DateTime.UtcNow, new List<ItemCestaDto>());
        _cestaService.Setup(s => s.GetAtualAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.GetCestaAtual();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task GetCestaHistorico_SempreRetorna200()
    {
        var dto = new CestaHistoricoDto(new List<CestaHistoricoItemDto>());
        _cestaService.Setup(s => s.GetHistoricoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.GetCestaHistorico();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
    }
    [Fact]
    public async Task GetCestaVigente_Encontrada_Retorna200()
    {
        var dto = new CestaResponseDto(1, "Top Five", true, DateTime.UtcNow, new List<ItemCestaDto>());
        _cestaService.Setup(s => s.GetVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.GetCestaVigente();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task GetCestaVigente_NaoEncontrada_Retorna404()
    {
        _cestaService.Setup(s => s.GetVigenteAsync(It.IsAny<CancellationToken>())).ReturnsAsync((CestaResponseDto?)null);
        var result = await _controller.GetCestaVigente();
        var notFound = Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task GetContaMasterCustodia_MotorIndisponivel_Retorna503()
    {
        _custodiaMasterClient.Setup(c => c.GetCustodiaMasterAsync(It.IsAny<CancellationToken>())).ReturnsAsync((CustodiaMasterResponseDto?)null);
        var result = await _controller.GetContaMasterCustodia();
        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(503, status.StatusCode);
    }
    [Fact]
    public async Task GetContaMasterCustodia_Sucesso_Retorna200()
    {
        var dto = new CustodiaMasterResponseDto(
            new ContaMasterDto(1, "MST-001", "MASTER"),
            new List<CustodiaMasterItemDto>(),
            0);
        _custodiaMasterClient.Setup(c => c.GetCustodiaMasterAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.GetContaMasterCustodia();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
}
