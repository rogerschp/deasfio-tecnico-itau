using Clientes.Service.Application.Entities;
using Clientes.Service.Application.Ports;
using Microsoft.EntityFrameworkCore;
namespace Clientes.Service.Infrastructure.Persistence;
public class ContaGraficaRepository : IContaGraficaRepository
{
    private readonly ClientesDbContext _db;
    public ContaGraficaRepository(ClientesDbContext db) => _db = db;
    public async Task<ContaGrafica?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var e = await _db.ContasGraficas.FindAsync([id], ct);
        return e is null ? null : new ContaGrafica { Id = e.Id, NumeroConta = e.NumeroConta, Tipo = e.Tipo, DataCriacao = e.DataCriacao, ClienteId = e.ClienteId };
    }
    public async Task<ContaGrafica> CriarAsync(long clienteId, CancellationToken ct = default)
    {
        var proximo = await _db.ContasGraficas.CountAsync(ct) + 1;
        var numero = $"FLH-{proximo:D6}";
        var e = new ContaGraficaEntity
        {
            NumeroConta = numero,
            Tipo = "FILHOTE",
            DataCriacao = DateTime.UtcNow,
            ClienteId = clienteId
        };
        _db.ContasGraficas.Add(e);
        await _db.SaveChangesAsync(ct);
        return new ContaGrafica { Id = e.Id, NumeroConta = e.NumeroConta, Tipo = e.Tipo, DataCriacao = e.DataCriacao, ClienteId = e.ClienteId };
    }
}
