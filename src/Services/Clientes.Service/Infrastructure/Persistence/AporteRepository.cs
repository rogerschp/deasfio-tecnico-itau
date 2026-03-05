using Clientes.Service.Application.Ports;
using Microsoft.EntityFrameworkCore;
namespace Clientes.Service.Infrastructure.Persistence;
public class AporteRepository : IAporteRepository
{
    private readonly ClientesDbContext _db;
    public AporteRepository(ClientesDbContext db) => _db = db;
    public async Task RegistrarAsync(long clienteId, DateOnly dataAporte, decimal valor, int parcela, CancellationToken ct = default)
    {
        _db.Aportes.Add(new AporteEntity
        {
            ClienteId = clienteId,
            DataAporte = dataAporte,
            Valor = valor,
            Parcela = parcela
        });
        await _db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<Aporte>> GetPorClienteOrdenadoPorDataAsync(long clienteId, CancellationToken ct = default)
    {
        var list = await _db.Aportes
            .Where(a => a.ClienteId == clienteId)
            .OrderBy(a => a.DataAporte)
            .ToListAsync(ct);
        return list.Select(a => new Aporte(a.ClienteId, a.DataAporte, a.Valor, a.Parcela)).ToList();
    }
}
