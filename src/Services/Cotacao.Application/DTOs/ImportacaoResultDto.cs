namespace Cotacao.Application.DTOs;

public record ImportacaoResultDto(
    DateOnly DataPregao,
    int RegistrosInseridos,
    bool PregaoJaExistia);
