using Microsoft.EntityFrameworkCore;
using MotorCompra.Service.Application.Ports;
namespace MotorCompra.Service.Infrastructure.Persistence;
public class CustodiaMasterRepository : ICustodiaMasterRepository
{
    private readonly MotorDbContext _db;
    public CustodiaMasterRepository(MotorDbContext db) => _db = db;
    public async Task<IReadOnlyDictionary<string, int>> GetSaldosPorTickerAsync(IReadOnlyList<string> tickers, CancellationToken ct = default)
    {
        if (tickers.Count == 0) return new Dictionary<string, int>();
        var list = await _db.CustodiaMaster
            .Where(c => tickers.Contains(c.Ticker))
            .Select(c => new { c.Ticker, c.Quantidade })
            .ToListAsync(ct);
        return list.ToDictionary(x => x.Ticker, x => x.Quantidade);
    }
    public async Task<IReadOnlyList<(string Ticker, int Quantidade)>> GetTodosResiduosAsync(CancellationToken ct = default)
    {
        var list = await _db.CustodiaMaster
            .Where(c => c.Quantidade > 0)
            .Select(c => new { c.Ticker, c.Quantidade })
            .ToListAsync(ct);
        return list.Select(x => (x.Ticker, x.Quantidade)).ToList();
    }
    public async Task DefinirResiduosAsync(IReadOnlyList<(string Ticker, int Quantidade)> residuos, CancellationToken ct = default)
    {
        foreach (var (ticker, quantidade) in residuos)
        {
            var existing = await _db.CustodiaMaster.FirstOrDefaultAsync(c => c.Ticker == ticker, ct);
            if (quantidade <= 0)
            {
                if (existing != null)
                    _db.CustodiaMaster.Remove(existing);
                continue;
            }
            if (existing != null)
                existing.Quantidade = quantidade;
            else
                _db.CustodiaMaster.Add(new CustodiaMasterEntity { Ticker = ticker, Quantidade = quantidade });
        }
        await _db.SaveChangesAsync(ct);
    }
}
