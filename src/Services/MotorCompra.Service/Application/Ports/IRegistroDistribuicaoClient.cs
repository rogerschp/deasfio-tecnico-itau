namespace MotorCompra.Service.Application.Ports;

/// <summary>
/// Porta para registrar a distribuição de ativos na custódia do cliente (Clientes Service).
/// </summary>
public interface IRegistroDistribuicaoClient
{
    Task RegistrarDistribuicaoAsync(long clienteId, long execucaoId, IReadOnlyList<ItemDistribuicaoDto> itens, CancellationToken ct = default);
}

public record ItemDistribuicaoDto(string Ticker, int Quantidade, decimal PrecoUnitario);
