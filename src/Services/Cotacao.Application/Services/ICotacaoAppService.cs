using Cotacao.Application.DTOs;
namespace Cotacao.Application.Services;

public interface ICotacaoAppService
{

    Task<CotacaoFechamentoDto?> GetFechamentoAsync(string ticker, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoFechamentoDto>> GetFechamentosAsync(
        IReadOnlyList<string> tickers,
        CancellationToken cancellationToken = default);

    Task<ImportacaoResultDto> ImportarArquivoAsync(string filePath, CancellationToken cancellationToken = default);
}
