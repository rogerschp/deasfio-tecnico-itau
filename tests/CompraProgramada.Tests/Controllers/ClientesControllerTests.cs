using Clientes.Service.Application.DTOs;
using Clientes.Service.Application.Services;
using Clientes.Service.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Contracts.Clientes;
namespace CompraProgramada.Tests.Controllers;
public class ClientesControllerTests
{
    private readonly Mock<IClienteAppService> _service;
    private readonly ClientesController _controller;
    private readonly ClientesInternalController _internalController;
    public ClientesControllerTests()
    {
        _service = new Mock<IClienteAppService>();
        _controller = new ClientesController(_service.Object);
        _internalController = new ClientesInternalController(_service.Object);
    }
    [Fact]
    public async Task Adesao_RequestNull_RetornaBadRequest()
    {
        var result = await _controller.Adesao(null!);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task Adesao_DadosInvalidos_RetornaBadRequest()
    {
        var result = await _controller.Adesao(new AdesaoRequest("", "", "", 100m));
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task Adesao_ValorInvalido_RetornaBadRequest()
    {
        _service.Setup(s => s.AderirAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Valor mensal mínimo"));
        var result = await _controller.Adesao(new AdesaoRequest("João", "12345678901", "j@x.com", 50m));
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task Adesao_Sucesso_Retorna201()
    {
        var dto = new AdesaoResponseDto(1, "João", "12345678901", "j@x.com", 3000m, true, DateTime.UtcNow, new ContaGraficaDto(10, "FLH-000001", "FILHOTE", DateTime.UtcNow));
        _service.Setup(s => s.AderirAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        var result = await _controller.Adesao(new AdesaoRequest("João", "12345678901", "j@x.com", 3000m));
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Same(dto, created.Value);
    }
    [Fact]
    public async Task AlterarValorMensal_RequestNull_RetornaBadRequest()
    {
        var result = await _controller.AlterarValorMensal(1, null!);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task AlterarValorMensal_ClienteNaoEncontrado_Retorna404()
    {
        _service.Setup(s => s.AlterarValorMensalAsync(999, 500m, It.IsAny<CancellationToken>())).ReturnsAsync((AlterarValorResponseDto?)null);
        var result = await _controller.AlterarValorMensal(999, new AlterarValorRequest(500m));
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task AlterarValorMensal_Sucesso_Retorna200()
    {
        var dto = new AlterarValorResponseDto(1, 100m, 500m, DateTime.UtcNow, "Valor atualizado.");
        _service.Setup(s => s.AlterarValorMensalAsync(1, 500m, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.AlterarValorMensal(1, new AlterarValorRequest(500m));
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task GetById_ClienteNaoEncontrado_Retorna404()
    {
        _service.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((AdesaoResponseDto?)null);
        var result = await _controller.GetById(999);
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task GetById_Sucesso_Retorna200()
    {
        var dto = new AdesaoResponseDto(1, "João", "12345678901", "j@x.com", 3000m, true, DateTime.UtcNow, null);
        _service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.GetById(1);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task Saida_ClienteNaoEncontrado_Retorna404()
    {
        _service.Setup(s => s.SairAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((SaidaResponseDto?)null);
        var result = await _controller.Saida(999);
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task Saida_Sucesso_Retorna200()
    {
        var dto = new SaidaResponseDto(1, "João", false, DateTime.UtcNow, "Saída registrada.");
        _service.Setup(s => s.SairAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.Saida(1);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task Saida_ClienteJaInativo_Retorna400()
    {
        _service.Setup(s => s.SairAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cliente já havia saído do produto."));
        var result = await _controller.Saida(1);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.NotNull(badRequest.Value);
    }
    [Fact]
    public async Task GetAtivos_Sucesso_Retorna200()
    {
        var list = new List<ClienteAtivoDto> { new(1, "João", "123", 3000m, 10), new(2, "Maria", "456", 2000m, 11) };
        _service.Setup(s => s.GetAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        var result = await _controller.GetAtivos(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        var resultList = Assert.IsAssignableFrom<IReadOnlyList<ClienteAtivoDto>>(ok.Value);
        Assert.Equal(2, resultList.Count);
    }
    [Fact]
    public async Task GetCarteira_Encontrado_Retorna200()
    {
        var resumo = new CarteiraResumoDto(10000m, 10500m, 500m, 5m);
        var dto = new CarteiraResponseDto(1, "João", "FLH-001", DateTime.UtcNow, resumo, new List<AtivoCarteiraDto>());
        _service.Setup(s => s.GetCarteiraAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.GetCarteira(1, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task GetCarteira_NaoEncontrado_Retorna404()
    {
        _service.Setup(s => s.GetCarteiraAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((CarteiraResponseDto?)null);
        var result = await _controller.GetCarteira(999, CancellationToken.None);
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task GetRentabilidade_Encontrado_Retorna200()
    {
        var resumo = new CarteiraResumoDto(10000m, 10500m, 500m, 5m);
        var dto = new RentabilidadeResponseDto(1, "João", DateTime.UtcNow, resumo, new List<HistoricoAporteDto>(), new List<EvolucaoCarteiraDto>());
        _service.Setup(s => s.GetRentabilidadeAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _controller.GetRentabilidade(1, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task GetRentabilidade_NaoEncontrado_Retorna404()
    {
        _service.Setup(s => s.GetRentabilidadeAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((RentabilidadeResponseDto?)null);
        var result = await _controller.GetRentabilidade(999, CancellationToken.None);
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task RegistrarDistribuicao_ComItens_Retorna204()
    {
        var request = new DistribuicaoRequestDto(1, 1, new List<ItemDistribuicaoDto> { new("PETR4", 10, 35m) }, new DateOnly(2026, 2, 5), 1000m, 1);
        _service.Setup(s => s.RegistrarDistribuicaoAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<IReadOnlyList<(string Ticker, int Quantidade, decimal PrecoUnitario)>>(), It.IsAny<DateOnly?>(), It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await _internalController.RegistrarDistribuicao(request, CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task RegistrarDistribuicao_ItensNull_Retorna204()
    {
        var result = await _internalController.RegistrarDistribuicao(new DistribuicaoRequestDto(1, 1, null!, null, null, null), CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task RegistrarDistribuicao_ItensVazios_Retorna204()
    {
        var result = await _internalController.RegistrarDistribuicao(new DistribuicaoRequestDto(1, 1, new List<ItemDistribuicaoDto>(), null, null, null), CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task VendaCustodia_QuantidadeInvalida_Retorna400()
    {
        var result = await _internalController.VendaCustodia(1, new VendaCustodiaRequest("PETR4", 0, 35m), CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task VendaCustodia_Sucesso_Retorna200()
    {
        var dto = new VendaCustodiaResultDto(350m, 50m);
        _service.Setup(s => s.VenderAtivoAsync(1, "PETR4", 10, 35m, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var result = await _internalController.VendaCustodia(1, new VendaCustodiaRequest("PETR4", 10, 35m), CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(dto, ok.Value);
    }
    [Fact]
    public async Task VendaCustodia_NaoEncontrado_Retorna404()
    {
        _service.Setup(s => s.VenderAtivoAsync(999, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ReturnsAsync((VendaCustodiaResultDto?)null);
        var result = await _internalController.VendaCustodia(999, new VendaCustodiaRequest("PETR4", 10, 35m), CancellationToken.None);
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }
    [Fact]
    public async Task CompraCustodia_QuantidadeInvalida_Retorna400()
    {
        var result = await _internalController.CompraCustodia(1, new CompraCustodiaRequest("PETR4", 0, 35m), CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }
    [Fact]
    public async Task CompraCustodia_Sucesso_Retorna204()
    {
        _service.Setup(s => s.RegistrarCompraRebalanceamentoAsync(1, "PETR4", 10, 35m, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await _internalController.CompraCustodia(1, new CompraCustodiaRequest("PETR4", 10, 35m), CancellationToken.None);
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task CompraCustodia_ClienteNaoEncontrado_Retorna404()
    {
        _service.Setup(s => s.RegistrarCompraRebalanceamentoAsync(999, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var result = await _internalController.CompraCustodia(999, new CompraCustodiaRequest("PETR4", 10, 35m), CancellationToken.None);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
