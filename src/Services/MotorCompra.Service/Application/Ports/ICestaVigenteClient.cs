namespace MotorCompra.Service.Application.Ports;

/// <summary>
/// Porta para obter a cesta Top Five vigente (Admin Service).
/// </summary>
public interface ICestaVigenteClient
{
    Task<CestaVigenteDto?> GetCestaVigenteAsync(CancellationToken ct = default);
}

/// <summary>
/// DTO da cesta vigente: 5 ativos com percentual (soma 100%).
/// </summary>
public record CestaVigenteDto(
    long CestaId,
    string Nome,
    IReadOnlyList<ItemCestaDto> Itens);

public record ItemCestaDto(string Ticker, decimal Percentual);
