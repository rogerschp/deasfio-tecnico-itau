using Cotacao.Domain;

namespace Cotacao.Application.Contracts;

/// <summary>
/// Repositório de cotações B3. Usa Dapper/SQL para leituras e bulk insert por performance.
/// </summary>
public interface ICotacaoRepository
{
    /// <summary>
    /// Obtém a cotação de fechamento do último pregão disponível para o ticker (consulta otimizada).
    /// </summary>
    Task<CotacaoB3?> GetFechamentoUltimoPregaoAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém cotações de fechamento do último pregão para vários tickers em uma única consulta.
    /// </summary>
    Task<IReadOnlyList<CotacaoB3>> GetFechamentosUltimoPregaoPorTickersAsync(
        IReadOnlyList<string> tickers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere em lote as cotações parseadas do COTAHIST (raw SQL/Dapper para performance).
    /// </summary>
    Task<int> BulkInsertAsync(IEnumerable<CotacaoB3> cotacoes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se já existem registros para a data de pregão (evitar duplicar importação).
    /// </summary>
    Task<bool> ExistePregaoAsync(DateOnly dataPregao, CancellationToken cancellationToken = default);
}
