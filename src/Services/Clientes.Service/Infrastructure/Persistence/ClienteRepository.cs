using Clientes.Service.Application.Entities;
using Clientes.Service.Application.Ports;
using Microsoft.EntityFrameworkCore;
namespace Clientes.Service.Infrastructure.Persistence;
public class ClienteRepository : IClienteRepository
{
    private readonly ClientesDbContext _db;
    public ClienteRepository(ClientesDbContext db) => _db = db;
    public async Task<Cliente?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var e = await _db.Clientes.FindAsync([id], ct);
        return e is null ? null : Map(e);
    }
    public async Task<Cliente?> GetByCpfAsync(string cpf, CancellationToken ct = default)
    {
        var norm = new string(cpf.Where(char.IsDigit).ToArray());
        var e = await _db.Clientes.FirstOrDefaultAsync(c => c.Cpf == norm, ct);
        return e is null ? null : Map(e);
    }
    public async Task<IReadOnlyList<Cliente>> GetAtivosAsync(CancellationToken ct = default)
    {
        var list = await _db.Clientes.Where(c => c.Ativo).ToListAsync(ct);
        return list.Select(Map).ToList();
    }
    public async Task<Cliente> SalvarAsync(Cliente cliente, CancellationToken ct = default)
    {
        if (cliente.Id == 0)
        {
            var e = new ClienteEntity
            {
                Nome = cliente.Nome,
                Cpf = cliente.Cpf,
                Email = cliente.Email,
                ValorMensal = cliente.ValorMensal,
                Ativo = cliente.Ativo,
                DataAdesao = cliente.DataAdesao,
                DataSaida = cliente.DataSaida,
                ContaGraficaId = cliente.ContaGraficaId
            };
            _db.Clientes.Add(e);
            await _db.SaveChangesAsync(ct);
            cliente.Id = e.Id;
            return cliente;
        }
        var existing = await _db.Clientes.FindAsync([cliente.Id], ct);
        if (existing != null)
        {
            existing.Nome = cliente.Nome;
            existing.Email = cliente.Email;
            existing.ValorMensal = cliente.ValorMensal;
            existing.Ativo = cliente.Ativo;
            existing.DataSaida = cliente.DataSaida;
            existing.ContaGraficaId = cliente.ContaGraficaId;
            await _db.SaveChangesAsync(ct);
        }
        return cliente;
    }
    private static Cliente Map(ClienteEntity e) => new()
    {
        Id = e.Id,
        Nome = e.Nome,
        Cpf = e.Cpf,
        Email = e.Email,
        ValorMensal = e.ValorMensal,
        Ativo = e.Ativo,
        DataAdesao = e.DataAdesao,
        DataSaida = e.DataSaida,
        ContaGraficaId = e.ContaGraficaId
    };
}
