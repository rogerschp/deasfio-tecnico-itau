namespace MotorCompra.Service.Application.Ports;

/// <summary>
/// Porta para listar clientes ativos no produto (Clientes Service).
/// </summary>
public interface IClientesAtivosClient
{
    Task<IReadOnlyList<ClienteAtivoDto>> GetClientesAtivosAsync(CancellationToken ct = default);
}

/// <summary>
/// DTO de cliente ativo para o motor: aporte 1/3 e dados para distribuição.
/// </summary>
public record ClienteAtivoDto(
    long ClienteId,
    string Nome,
    string Cpf,
    decimal ValorMensal,
    long ContaGraficaId);
