namespace Admin.Service.Application.Ports;

public interface ICotacaoFechamentoClient
{
    Task<IReadOnlyList<CotacaoFechamentoDto>> GetFechamentosAsync(IReadOnlyList<string> tickers, CancellationToken ct = default);
}
public record CotacaoFechamentoDto(string Ticker, decimal PrecoFechamento);
