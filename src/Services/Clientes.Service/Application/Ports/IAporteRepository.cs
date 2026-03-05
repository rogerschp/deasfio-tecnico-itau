namespace Clientes.Service.Application.Ports;

public record Aporte(long ClienteId, DateOnly DataAporte, decimal Valor, int Parcela);
public interface IAporteRepository
{
    Task RegistrarAsync(long clienteId, DateOnly dataAporte, decimal valor, int parcela, CancellationToken ct = default);
    Task<IReadOnlyList<Aporte>> GetPorClienteOrdenadoPorDataAsync(long clienteId, CancellationToken ct = default);
}
