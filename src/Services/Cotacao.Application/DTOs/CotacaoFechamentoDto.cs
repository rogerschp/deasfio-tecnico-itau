namespace Cotacao.Application.DTOs;

/// <summary>
/// DTO de cotação de fechamento para exposição na API.
/// </summary>
public record CotacaoFechamentoDto(
    string Ticker,
    DateOnly DataPregao,
    decimal PrecoFechamento);
