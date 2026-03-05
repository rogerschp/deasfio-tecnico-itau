namespace Admin.Service.Application.Ports;

public interface ICustodiaMasterClient
{
    Task<CustodiaMasterResponseDto?> GetCustodiaMasterAsync(CancellationToken ct = default);
}
public record CustodiaMasterResponseDto(ContaMasterDto ContaMaster, IReadOnlyList<CustodiaMasterItemDto> Custodia, decimal ValorTotalResiduo);
public record ContaMasterDto(long Id, string NumeroConta, string Tipo);
public record CustodiaMasterItemDto(string Ticker, int Quantidade, decimal PrecoMedio, decimal ValorAtual, string Origem);
