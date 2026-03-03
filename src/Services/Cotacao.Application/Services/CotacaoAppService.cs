using Cotacao.Application.Contracts;
using Cotacao.Application.DTOs;
using Cotacao.Domain;

namespace Cotacao.Application.Services;

/// <inheritdoc />
public class CotacaoAppService : ICotacaoAppService
{
    private readonly ICotacaoRepository _repository;
    private readonly ICotahistParser _parser;

    public CotacaoAppService(ICotacaoRepository repository, ICotahistParser parser)
    {
        _repository = repository;
        _parser = parser;
    }

    /// <inheritdoc />
    public async Task<CotacaoFechamentoDto?> GetFechamentoAsync(string ticker, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            return null;

        var entity = await _repository.GetFechamentoUltimoPregaoAsync(ticker.Trim().ToUpperInvariant(), cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CotacaoFechamentoDto>> GetFechamentosAsync(
        IReadOnlyList<string> tickers,
        CancellationToken cancellationToken = default)
    {
        if (tickers is null || tickers.Count == 0)
            return Array.Empty<CotacaoFechamentoDto>();

        var normalized = tickers
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
            return Array.Empty<CotacaoFechamentoDto>();

        var entities = await _repository.GetFechamentosUltimoPregaoPorTickersAsync(normalized, cancellationToken);
        return entities.Select(ToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<ImportacaoResultDto> ImportarArquivoAsync(string caminhoArquivo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(caminhoArquivo) || !File.Exists(caminhoArquivo))
            throw new FileNotFoundException("Arquivo COTAHIST não encontrado.", caminhoArquivo);

        var cotacoes = new List<CotacaoB3>();
        DateOnly? dataPregao = null;

        await foreach (var c in _parser.ParseFromFileAsync(caminhoArquivo, cancellationToken).WithCancellation(cancellationToken))
        {
            cotacoes.Add(c);
            dataPregao ??= c.DataPregao;
        }

        if (cotacoes.Count == 0)
            return new ImportacaoResultDto(dataPregao ?? DateOnly.FromDateTime(DateTime.Today), 0, false);

        var primeiraData = cotacoes[0].DataPregao;
        var pregaoJaExistia = await _repository.ExistePregaoAsync(primeiraData, cancellationToken);
        if (pregaoJaExistia)
            return new ImportacaoResultDto(primeiraData, 0, true);

        var inseridos = await _repository.BulkInsertAsync(cotacoes, cancellationToken);
        return new ImportacaoResultDto(primeiraData, inseridos, false);
    }

    private static CotacaoFechamentoDto ToDto(CotacaoB3 entity) =>
        new(entity.Ticker, entity.DataPregao, entity.PrecoFechamento);
}
