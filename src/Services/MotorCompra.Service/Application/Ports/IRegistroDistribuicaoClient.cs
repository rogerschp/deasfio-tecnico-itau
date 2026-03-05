namespace MotorCompra.Service.Application.Ports;

public interface IRegistroDistribuicaoClient
{
    Task RegistrarDistribuicaoAsync(long clienteId, long execucaoId, IReadOnlyList<ItemDistribuicaoDto> itens, DateOnly? dataAporte = null, decimal? valorAporte = null, int? parcela = null, CancellationToken ct = default);
}
public record ItemDistribuicaoDto(string Ticker, int Quantidade, decimal PrecoUnitario);
