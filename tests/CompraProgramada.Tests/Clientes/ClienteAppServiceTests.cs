using Clientes.Service.Application.Entities;
using Clientes.Service.Application.Ports;
using Clientes.Service.Application.Services;
using Moq;
namespace CompraProgramada.Tests.Clientes;
public class ClienteAppServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepo;
    private readonly Mock<IContaGraficaRepository> _contaRepo;
    private readonly Mock<ICustodiaRepository> _custodiaRepo;
    private readonly Mock<IAporteRepository> _aporteRepo;
    private readonly ClienteAppService _sut;
    public ClienteAppServiceTests()
    {
        _clienteRepo = new Mock<IClienteRepository>();
        _contaRepo = new Mock<IContaGraficaRepository>();
        _custodiaRepo = new Mock<ICustodiaRepository>();
        _aporteRepo = new Mock<IAporteRepository>();
        _sut = new ClienteAppService(_clienteRepo.Object, _contaRepo.Object, _custodiaRepo.Object, _aporteRepo.Object);
    }
    [Fact]
    public async Task AderirAsync_ValorMenorQue100_LancaArgumentException()
    {
        _clienteRepo.Setup(r => r.GetByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.AderirAsync("João", "12345678901", "joao@email.com", 50m));
    }
    [Fact]
    public async Task AderirAsync_CpfDuplicado_LancaInvalidOperationException()
    {
        _clienteRepo.Setup(r => r.GetByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cliente { Id = 1, Cpf = "12345678901" });
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.AderirAsync("João", "123.456.789-01", "joao@email.com", 3000m));
    }
    [Fact]
    public async Task AderirAsync_DadosValidos_CriaClienteEConta()
    {
        _clienteRepo.Setup(r => r.GetByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        _clienteRepo.Setup(r => r.SalvarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken _) => { c.Id = 1; return c; });
        _contaRepo.Setup(r => r.CriarAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContaGrafica { Id = 10, NumeroConta = "FLH-000001", Tipo = "FILHOTE", DataCriacao = DateTime.UtcNow, ClienteId = 1 });
        var result = await _sut.AderirAsync("João Silva", "12345678901", "joao@email.com", 3000m);
        Assert.NotNull(result);
        Assert.Equal(1, result.ClienteId);
        Assert.Equal("João Silva", result.Nome);
        Assert.Equal(3000m, result.ValorMensal);
        Assert.True(result.Ativo);
        Assert.NotNull(result.ContaGrafica);
        Assert.Equal(10, result.ContaGrafica.Id);
        Assert.Equal("FLH-000001", result.ContaGrafica.NumeroConta);
        _clienteRepo.Verify(r => r.SalvarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _contaRepo.Verify(r => r.CriarAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task SairAsync_ClienteInexistente_RetornaNull()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        var result = await _sut.SairAsync(999);
        Assert.Null(result);
    }
    [Fact]
    public async Task SairAsync_ClienteExistente_DesativaERetornaDto()
    {
        var cliente = new Cliente { Id = 1, Nome = "João", Ativo = true };
        _clienteRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _clienteRepo.Setup(r => r.SalvarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente c, CancellationToken _) => c);
        var result = await _sut.SairAsync(1);
        Assert.NotNull(result);
        Assert.False(result.Ativo);
        Assert.NotEqual(default, result.DataSaida);
        _clienteRepo.Verify(r => r.SalvarAsync(It.Is<Cliente>(c => !c.Ativo && c.DataSaida.HasValue), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task AlterarValorMensalAsync_ClienteInexistente_RetornaNull()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        var result = await _sut.AlterarValorMensalAsync(999, 5000m);
        Assert.Null(result);
    }
    [Fact]
    public async Task AlterarValorMensalAsync_ValorMenorQue100_LancaArgumentException()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = 1 });
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AlterarValorMensalAsync(1, 50m));
    }
    [Fact]
    public async Task GetAtivosAsync_QuandoNenhumAtivo_RetornaListaVazia()
    {
        _clienteRepo.Setup(r => r.GetAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Cliente>());
        var result = await _sut.GetAtivosAsync();
        Assert.Empty(result);
    }
    [Fact]
    public async Task GetAtivosAsync_QuandoClientesSemConta_NaoIncluiNaLista()
    {
        _clienteRepo.Setup(r => r.GetAtivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cliente> { new() { Id = 1, Nome = "A", Cpf = "1", ValorMensal = 1000m, ContaGraficaId = null } });
        var result = await _sut.GetAtivosAsync();
        Assert.Empty(result);
    }
    [Fact]
    public async Task GetAtivosAsync_QuandoClienteComConta_RetornaDto()
    {
        _clienteRepo.Setup(r => r.GetAtivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cliente> { new() { Id = 1, Nome = "João", Cpf = "12345678901", ValorMensal = 3000m, ContaGraficaId = 10 } });
        _contaRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContaGrafica { Id = 10, NumeroConta = "FLH-000001", Tipo = "FILHOTE", DataCriacao = DateTime.UtcNow, ClienteId = 1 });
        var result = await _sut.GetAtivosAsync();
        Assert.Single(result);
        Assert.Equal(1, result[0].ClienteId);
        Assert.Equal("João", result[0].Nome);
        Assert.Equal(3000m, result[0].ValorMensal);
        Assert.Equal(10, result[0].ContaGraficaId);
    }
    [Fact]
    public async Task RegistrarDistribuicaoAsync_ClienteInexistente_NaoChamaCustodia()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        await _sut.RegistrarDistribuicaoAsync(999, 1, new List<(string, int, decimal)> { ("PETR4", 10, 35m) });
        _custodiaRepo.Verify(r => r.AdicionarOuAtualizarAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task RegistrarDistribuicaoAsync_ClienteComConta_ChamaAdicionarOuAtualizar()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cliente { Id = 1, ContaGraficaId = 10 });
        var itens = new List<(string, int, decimal)> { ("PETR4", 8, 35.00m), ("VALE3", 4, 62.00m) };
        await _sut.RegistrarDistribuicaoAsync(1, 1, itens);
        _custodiaRepo.Verify(r => r.AdicionarOuAtualizarAsync(10, "PETR4", 8, 35.00m, It.IsAny<CancellationToken>()), Times.Once);
        _custodiaRepo.Verify(r => r.AdicionarOuAtualizarAsync(10, "VALE3", 4, 62.00m, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task GetCarteiraAsync_ClienteComCustodia_RetornaCarteira()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cliente { Id = 1, Nome = "João", ContaGraficaId = 10 });
        _contaRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContaGrafica { Id = 10, NumeroConta = "FLH-001", Tipo = "FILHOTE", DataCriacao = DateTime.UtcNow, ClienteId = 1 });
        _custodiaRepo.Setup(r => r.GetPorContaAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustodiaFilhote> { new() { Ticker = "PETR4", Quantidade = 10, PrecoMedio = 35m }, new() { Ticker = "VALE3", Quantidade = 5, PrecoMedio = 62m } });
        var result = await _sut.GetCarteiraAsync(1);
        Assert.NotNull(result);
        Assert.Equal(1, result.ClienteId);
        Assert.Equal("João", result.Nome);
        Assert.Equal(2, result.Ativos.Count);
        Assert.True(result.Resumo.ValorTotalInvestido > 0);
    }
    [Fact]
    public async Task GetCarteiraAsync_ClienteSemConta_RetornaNull()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cliente { Id = 1, ContaGraficaId = null });
        var result = await _sut.GetCarteiraAsync(1);
        Assert.Null(result);
    }
    [Fact]
    public async Task GetRentabilidadeAsync_ClienteComCarteiraEAportes_RetornaRentabilidade()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cliente { Id = 1, Nome = "Maria", ContaGraficaId = 10 });
        _contaRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContaGrafica { Id = 10, NumeroConta = "FLH-001", Tipo = "FILHOTE", DataCriacao = DateTime.UtcNow, ClienteId = 1 });
        _custodiaRepo.Setup(r => r.GetPorContaAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustodiaFilhote> { new() { Ticker = "PETR4", Quantidade = 10, PrecoMedio = 35m } });
        _aporteRepo.Setup(r => r.GetPorClienteOrdenadoPorDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Aporte> { new(1, new DateOnly(2026, 2, 5), 1000m, 1) });
        var result = await _sut.GetRentabilidadeAsync(1);
        Assert.NotNull(result);
        Assert.Equal(1, result.ClienteId);
        Assert.Equal("Maria", result.Nome);
        Assert.Single(result.HistoricoAportes);
        Assert.Single(result.EvolucaoCarteira);
    }
    [Fact]
    public async Task GetRentabilidadeAsync_ClienteSemCarteira_RetornaNull()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        var result = await _sut.GetRentabilidadeAsync(999);
        Assert.Null(result);
    }
    [Fact]
    public async Task VenderAtivoAsync_QuantidadeInvalida_RetornaNull()
    {
        var result = await _sut.VenderAtivoAsync(1, "PETR4", 0, 35m);
        Assert.Null(result);
    }
    [Fact]
    public async Task RegistrarCompraRebalanceamentoAsync_ClienteInexistente_RetornaFalse()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        var result = await _sut.RegistrarCompraRebalanceamentoAsync(999, "PETR4", 10, 35m);
        Assert.False(result);
    }
    [Fact]
    public async Task VenderAtivoAsync_ClienteComPosicao_RetornaValorELucro()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cliente { Id = 1, ContaGraficaId = 10 });
        _custodiaRepo.Setup(r => r.VenderAsync(10, "PETR4", 5, 40m, It.IsAny<CancellationToken>()))
            .ReturnsAsync((200m, 25m));
        var result = await _sut.VenderAtivoAsync(1, "PETR4", 5, 40m);
        Assert.NotNull(result);
        Assert.Equal(200m, result.ValorVenda);
        Assert.Equal(25m, result.Lucro);
    }
    [Fact]
    public async Task RegistrarCompraRebalanceamentoAsync_ClienteComConta_RetornaTrue()
    {
        _clienteRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cliente { Id = 1, ContaGraficaId = 10 });
        _custodiaRepo.Setup(r => r.AdicionarOuAtualizarAsync(10, "PETR4", 10, 35m, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await _sut.RegistrarCompraRebalanceamentoAsync(1, "PETR4", 10, 35m);
        Assert.True(result);
    }
}
