using Admin.Service.Application.Entities;
using Admin.Service.Application.Ports;
using Microsoft.EntityFrameworkCore;
namespace Admin.Service.Infrastructure.Persistence;
public class CestaRepository : ICestaRepository
{
    private readonly AdminDbContext _db;
    public CestaRepository(AdminDbContext db) => _db = db;
    public async Task<Cesta?> GetAtivaAsync(CancellationToken ct = default)
    {
        var entity = await _db.Cestas
            .Include(c => c.Itens)
            .Where(c => c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .FirstOrDefaultAsync(ct);
        return entity is null ? null : MapToDomain(entity);
    }
    public async Task<Cesta?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _db.Cestas.Include(c => c.Itens).FirstOrDefaultAsync(c => c.Id == id, ct);
        return entity is null ? null : MapToDomain(entity);
    }
    public async Task<IReadOnlyList<Cesta>> GetHistoricoAsync(CancellationToken ct = default)
    {
        var list = await _db.Cestas.Include(c => c.Itens).OrderByDescending(c => c.DataCriacao).ToListAsync(ct);
        var result = new List<Cesta>(list.Count);
        for (var i = 0; i < list.Count; i++)
            result.Add(MapToDomain(list[i]));
        return result;
    }
    public async Task<Cesta> SalvarAsync(Cesta cesta, CancellationToken ct = default)
    {
        var entity = new CestaEntity
        {
            Nome = cesta.Nome,
            Ativa = cesta.Ativa,
            DataCriacao = cesta.DataCriacao
        };
        foreach (var i in cesta.Itens)
            entity.Itens.Add(new ItemCestaEntity { Ticker = i.Ticker, Percentual = i.Percentual });
        _db.Cestas.Add(entity);
        await _db.SaveChangesAsync(ct);
        cesta.Id = entity.Id;
        foreach (var (item, entityItem) in cesta.Itens.Zip(entity.Itens, (a, b) => (a, b)))
            item.Id = entityItem.Id;
        return cesta;
    }
    public async Task DesativarAsync(long cestaId, DateTime dataDesativacao, CancellationToken ct = default)
    {
        var entity = await _db.Cestas.FindAsync([cestaId], ct);
        if (entity != null)
        {
            entity.Ativa = false;
            entity.DataDesativacao = dataDesativacao;
            await _db.SaveChangesAsync(ct);
        }
    }
    private static Cesta MapToDomain(CestaEntity e)
    {
        var itens = new List<ItemCesta>(e.Itens.Count);
        foreach (var i in e.Itens)
            itens.Add(new ItemCesta { Id = i.Id, CestaId = i.CestaId, Ticker = i.Ticker, Percentual = i.Percentual });
        return new Cesta
        {
            Id = e.Id,
            Nome = e.Nome,
            Ativa = e.Ativa,
            DataCriacao = e.DataCriacao,
            DataDesativacao = e.DataDesativacao,
            Itens = itens
        };
    }
}
