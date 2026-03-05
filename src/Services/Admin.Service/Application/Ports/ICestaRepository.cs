using Admin.Service.Application.Entities;
namespace Admin.Service.Application.Ports;
public interface ICestaRepository
{
    Task<Cesta?> GetAtivaAsync(CancellationToken ct = default);
    Task<Cesta?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<Cesta>> GetHistoricoAsync(CancellationToken ct = default);
    Task<Cesta> SalvarAsync(Cesta cesta, CancellationToken ct = default);
    Task DesativarAsync(long cestaId, DateTime dataDesativacao, CancellationToken ct = default);
}
