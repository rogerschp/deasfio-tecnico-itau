using Clientes.Service.Application.Entities;
using Clientes.Service.Application.Ports;
using Microsoft.EntityFrameworkCore;
namespace Clientes.Service.Infrastructure.Persistence;
public class CustodiaRepository : ICustodiaRepository
{
    private readonly ClientesDbContext _db;
    public CustodiaRepository(ClientesDbContext db) => _db = db;
    public async Task<IReadOnlyList<CustodiaFilhote>> GetPorContaAsync(long contaGraficaId, CancellationToken ct = default)
    {
        var list = await _db.CustodiasFilhote.Where(c => c.ContaGraficaId == contaGraficaId).ToListAsync(ct);
        return list.Select(Map).ToList();
    }
    public async Task<CustodiaFilhote?> GetPorContaETickerAsync(long contaGraficaId, string ticker, CancellationToken ct = default)
    {
        var e = await _db.CustodiasFilhote.FirstOrDefaultAsync(c => c.ContaGraficaId == contaGraficaId && c.Ticker == ticker, ct);
        return e is null ? null : Map(e);
    }
    public async Task AdicionarOuAtualizarAsync(long contaGraficaId, string ticker, int quantidade, decimal precoUnitario, CancellationToken ct = default)
    {
        var existing = await _db.CustodiasFilhote.FirstOrDefaultAsync(c => c.ContaGraficaId == contaGraficaId && c.Ticker == ticker, ct);
        if (existing is null)
        {
            _db.CustodiasFilhote.Add(new CustodiaFilhoteEntity
            {
                ContaGraficaId = contaGraficaId,
                Ticker = ticker,
                Quantidade = quantidade,
                PrecoMedio = precoUnitario
            });
        }
        else
        {
            var qtdTotal = existing.Quantidade + quantidade;
            existing.PrecoMedio = (existing.Quantidade * existing.PrecoMedio + quantidade * precoUnitario) / qtdTotal;
            existing.Quantidade = qtdTotal;
        }
        await _db.SaveChangesAsync(ct);
    }
    public async Task<(decimal ValorVenda, decimal Lucro)?> VenderAsync(long contaGraficaId, string ticker, int quantidade, decimal precoVenda, CancellationToken ct = default)
    {
        var existing = await _db.CustodiasFilhote.FirstOrDefaultAsync(c => c.ContaGraficaId == contaGraficaId && c.Ticker == ticker, ct);
        if (existing is null || existing.Quantidade < quantidade)
            return null;
        var valorVenda = quantidade * precoVenda;
        var lucro = quantidade * (precoVenda - existing.PrecoMedio);
        existing.Quantidade -= quantidade;
        if (existing.Quantidade <= 0)
            _db.CustodiasFilhote.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return (valorVenda, lucro);
    }
    private static CustodiaFilhote Map(CustodiaFilhoteEntity e) => new()
    {
        Id = e.Id,
        ContaGraficaId = e.ContaGraficaId,
        Ticker = e.Ticker,
        Quantidade = e.Quantidade,
        PrecoMedio = e.PrecoMedio
    };
}
