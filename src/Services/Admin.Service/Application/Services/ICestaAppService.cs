using Admin.Service.Application.DTOs;
namespace Admin.Service.Application.Services;
public interface ICestaAppService
{
    Task<CestaResponseDto> CadastrarOuAlterarAsync(string name, IReadOnlyList<(string Ticker, decimal Percentual)> items, CancellationToken ct = default);
    Task<CestaResponseDto?> GetAtualAsync(CancellationToken ct = default);
    Task<CestaResponseDto?> GetVigenteAsync(CancellationToken ct = default);
    Task<CestaHistoricoDto> GetHistoricoAsync(CancellationToken ct = default);
}
