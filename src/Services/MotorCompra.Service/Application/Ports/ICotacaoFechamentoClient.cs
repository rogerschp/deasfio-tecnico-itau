namespace MotorCompra.Service.Application.Ports;

/// <summary>
/// Porta para obter cotações de fechamento do último pregão (Cotação Service).
/// </summary>
public interface ICotacaoFechamentoClient
{
    Task<IReadOnlyList<CotacaoFechamentoDto>> GetFechamentosAsync(IReadOnlyList<string> tickers, CancellationToken ct = default);
}

/// <summary>
/// DTO de cotação de fechamento (compatível com resposta da API de Cotação).
/// </summary>
public record CotacaoFechamentoDto(string Ticker, DateOnly DataPregao, decimal PrecoFechamento);
