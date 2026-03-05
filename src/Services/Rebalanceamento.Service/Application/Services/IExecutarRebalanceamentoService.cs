using Rebalanceamento.Service.Application.Ports;
namespace Rebalanceamento.Service.Application.Services;

public static class RebalanceamentoConstants
{
    public const decimal LimiarDesvioPadrao = 5m;
    public const decimal LimiarPercentualMaximo = 100m;
}

public interface IExecutarRebalanceamentoService
{
    Task<ResultadoRebalanceamentoDto?> ExecutarPorMudancaCestaAsync(
        IReadOnlyList<ItemCestaDto> previousBasket,
        IReadOnlyList<ItemCestaDto> newBasket,
        CancellationToken ct = default);

    Task<ResultadoRebalanceamentoDto?> ExecutarPorDesvioAsync(decimal thresholdPercentage = RebalanceamentoConstants.LimiarDesvioPadrao, CancellationToken ct = default);
}
public record ResultadoRebalanceamentoDto(
    DateTime DataExecucao,
    int ClientesProcessados,
    int VendasRealizadas,
    int ComprasRealizadas,
    IReadOnlyList<string> Erros);
