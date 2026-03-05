namespace Cotacao.Application.DTOs;

public record CotacaoFechamentoDto(
    string Ticker,
    DateOnly DataPregao,
    decimal PrecoFechamento);
