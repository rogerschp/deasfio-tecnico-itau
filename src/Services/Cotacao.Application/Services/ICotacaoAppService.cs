using Cotacao.Application.DTOs;

namespace Cotacao.Application.Services;

/// <summary>
/// Serviço de aplicação de cotações: orquestra parser, repositório e regras.
/// </summary>
public interface ICotacaoAppService
{
    /// <summary>
    /// Obtém a cotação de fechamento do último pregão para um ticker.
    /// </summary>
    Task<CotacaoFechamentoDto?> GetFechamentoAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém cotações de fechamento do último pregão para vários tickers.
    /// </summary>
    Task<IReadOnlyList<CotacaoFechamentoDto>> GetFechamentosAsync(
        IReadOnlyList<string> tickers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Importa arquivo COTAHIST da B3: parse + persistência em lote.
    /// Retorna quantidade de registros inseridos. Ignora se o pregão já foi importado.
    /// </summary>
    Task<ImportacaoResultDto> ImportarArquivoAsync(string caminhoArquivo, CancellationToken cancellationToken = default);
}
