namespace MotorCompra.Service.Application.Ports;

public interface ICustodiaMasterRepository
{
    Task<IReadOnlyDictionary<string, int>> GetSaldosPorTickerAsync(IReadOnlyList<string> tickers, CancellationToken ct = default);
    Task<IReadOnlyList<(string Ticker, int Quantidade)>> GetTodosResiduosAsync(CancellationToken ct = default);
    Task DefinirResiduosAsync(IReadOnlyList<(string Ticker, int Quantidade)> residuos, CancellationToken ct = default);
}
