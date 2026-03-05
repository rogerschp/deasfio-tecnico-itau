namespace MotorCompra.Service.Application.Ports;

public interface ICestaVigenteClient
{
    Task<CestaVigenteDto?> GetCestaVigenteAsync(CancellationToken ct = default);
}

public record CestaVigenteDto(
    long CestaId,
    string Nome,
    IReadOnlyList<ItemCestaDto> Itens);
public record ItemCestaDto(string Ticker, decimal Percentual);
