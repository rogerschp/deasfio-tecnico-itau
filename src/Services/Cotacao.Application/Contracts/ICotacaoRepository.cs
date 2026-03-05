using Cotacao.Domain;
namespace Cotacao.Application.Contracts;

public interface ICotacaoRepository
{

    Task<CotacaoB3?> GetFechamentoUltimoPregaoAsync(string ticker, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoB3>> GetFechamentosUltimoPregaoPorTickersAsync(
        IReadOnlyList<string> tickers,
        CancellationToken cancellationToken = default);

    Task<int> BulkInsertAsync(IEnumerable<CotacaoB3> quotes, CancellationToken cancellationToken = default);

    Task<bool> ExistePregaoAsync(DateOnly tradingDate, CancellationToken cancellationToken = default);
}
