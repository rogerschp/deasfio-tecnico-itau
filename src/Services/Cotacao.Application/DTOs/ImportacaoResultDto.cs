namespace Cotacao.Application.DTOs;

/// <summary>
/// Resultado da importação de um arquivo COTAHIST.
/// </summary>
public record ImportacaoResultDto(
    DateOnly DataPregao,
    int RegistrosInseridos,
    bool PregaoJaExistia);
