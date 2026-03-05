namespace Rebalanceamento.Service.Application.Ports;

public record ItemCestaDto(string Ticker, decimal Percentual);

public record ClienteAtivoDto(long ClienteId, string Nome, string Cpf, decimal ValorMensal, long ContaGraficaId);

public record PosicaoCustodiaDto(string Ticker, int Quantidade, decimal PrecoMedio);

public record CarteiraClienteDto(long ClienteId, IReadOnlyList<PosicaoCustodiaDto> Posicoes);

public record VendaCustodiaResultDto(decimal ValorVenda, decimal Lucro);

public interface IClientesRebalanceamentoClient
{
    Task<IReadOnlyList<ClienteAtivoDto>> GetClientesAtivosAsync(CancellationToken ct = default);
    Task<CarteiraClienteDto?> GetCarteiraAsync(long clienteId, CancellationToken ct = default);
    Task<VendaCustodiaResultDto?> VenderAsync(long clienteId, string ticker, int quantidade, decimal precoVenda, CancellationToken ct = default);
    Task<bool> ComprarAsync(long clienteId, string ticker, int quantidade, decimal precoUnitario, CancellationToken ct = default);
}

public interface ICestaVigenteClient
{
    Task<CestaVigenteDto?> GetCestaVigenteAsync(CancellationToken ct = default);
}
public record CestaVigenteDto(long CestaId, string Nome, IReadOnlyList<ItemCestaDto> Itens);

public interface ICotacaoFechamentoClient
{
    Task<IReadOnlyList<CotacaoFechamentoDto>> GetFechamentosAsync(IReadOnlyList<string> tickers, CancellationToken ct = default);
}
public record CotacaoFechamentoDto(string Ticker, decimal PrecoFechamento);

public interface IVendaRebalanceamentoRepository
{
    Task RegistrarVendaAsync(long clienteId, string cpf, string ticker, int quantidade, decimal precoVenda, decimal precoMedio, decimal lucro, DateTime data, CancellationToken ct = default);
    Task<(decimal TotalVendasMes, decimal LucroLiquidoMes)> GetTotalVendasELucroClienteNoMesAsync(long clienteId, int ano, int mes, CancellationToken ct = default);
}
