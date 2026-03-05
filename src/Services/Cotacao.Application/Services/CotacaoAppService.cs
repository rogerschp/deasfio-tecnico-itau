using Cotacao.Application.Contracts;
using Cotacao.Application.DTOs;
using Cotacao.Domain;
namespace Cotacao.Application.Services;

public class CotacaoAppService : ICotacaoAppService
{
    private readonly ICotacaoRepository _repository;
    private readonly ICotahistParser _parser;
    public CotacaoAppService(ICotacaoRepository repository, ICotahistParser parser)
    {
        _repository = repository;
        _parser = parser;
    }

    public async Task<CotacaoFechamentoDto?> GetFechamentoAsync(string ticker, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            return null;
        var entity = await _repository.GetFechamentoUltimoPregaoAsync(ticker.Trim().ToUpperInvariant(), cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<IReadOnlyList<CotacaoFechamentoDto>> GetFechamentosAsync(
        IReadOnlyList<string> tickers,
        CancellationToken cancellationToken = default)
    {
        if (tickers is null || tickers.Count == 0)
            return Array.Empty<CotacaoFechamentoDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalized = new List<string>(tickers.Count);
        for (var i = 0; i < tickers.Count; i++)
        {
            var t = tickers[i];
            if (string.IsNullOrWhiteSpace(t)) continue;
            var key = t.Trim().ToUpperInvariant();
            if (seen.Add(key))
                normalized.Add(key);
        }
        if (normalized.Count == 0)
            return Array.Empty<CotacaoFechamentoDto>();
        var entities = await _repository.GetFechamentosUltimoPregaoPorTickersAsync(normalized, cancellationToken);
        var result = new List<CotacaoFechamentoDto>(entities.Count);
        foreach (var e in entities)
            result.Add(ToDto(e));
        return result;
    }

    public async Task<ImportacaoResultDto> ImportarArquivoAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Arquivo COTAHIST não encontrado.", filePath);
        var quotes = new List<CotacaoB3>();
        DateOnly? tradingDate = null;
        await foreach (var c in _parser.ParseFromFileAsync(filePath, cancellationToken).WithCancellation(cancellationToken))
        {
            quotes.Add(c);
            tradingDate ??= c.DataPregao;
        }
        if (quotes.Count == 0)
            return new ImportacaoResultDto(tradingDate ?? DateOnly.FromDateTime(DateTime.Today), 0, false);
        var firstDate = quotes[0].DataPregao;
        var tradingDateExists = await _repository.ExistePregaoAsync(firstDate, cancellationToken);
        if (tradingDateExists)
            return new ImportacaoResultDto(firstDate, 0, true);
        var inserted = await _repository.BulkInsertAsync(quotes, cancellationToken);
        return new ImportacaoResultDto(firstDate, inserted, false);
    }
    private static CotacaoFechamentoDto ToDto(CotacaoB3 entity) =>
        new(entity.Ticker, entity.DataPregao, entity.PrecoFechamento);
}
