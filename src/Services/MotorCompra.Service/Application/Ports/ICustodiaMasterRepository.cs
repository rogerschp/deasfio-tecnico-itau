namespace MotorCompra.Service.Application.Ports;

/// <summary>
/// Porta de leitura/escrita da custódia master (resíduos por ticker).
/// </summary>
public interface ICustodiaMasterRepository
{
    Task<IReadOnlyDictionary<string, int>> GetSaldosPorTickerAsync(IReadOnlyList<string> tickers, CancellationToken ct = default);
    /// <summary>Define os resíduos na custódia master após uma execução (sobra após distribuição).</summary>
    Task DefinirResiduosAsync(IReadOnlyList<(string Ticker, int Quantidade)> residuos, CancellationToken ct = default);
}
