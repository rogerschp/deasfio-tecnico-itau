namespace MotorCompra.Service.Application.Ports;

public interface IClientesAtivosClient
{
    Task<IReadOnlyList<ClienteAtivoDto>> GetClientesAtivosAsync(CancellationToken ct = default);
}

public record ClienteAtivoDto(
    long ClienteId,
    string Nome,
    string Cpf,
    decimal ValorMensal,
    long ContaGraficaId);
