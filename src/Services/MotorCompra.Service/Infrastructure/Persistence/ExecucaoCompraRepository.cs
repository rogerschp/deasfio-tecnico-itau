using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MotorCompra.Service.Application.Entities;
using MotorCompra.Service.Application.Ports;
namespace MotorCompra.Service.Infrastructure.Persistence;
public class ExecucaoCompraRepository : IExecucaoCompraRepository
{
    private readonly MotorDbContext _db;
    public ExecucaoCompraRepository(MotorDbContext db) => _db = db;
    public async Task<bool> JaExecutouNaDataAsync(DateOnly referenceDate, CancellationToken ct = default)
    {
        return await _db.ExecucoesCompra.AnyAsync(e => e.DataReferencia == referenceDate, ct);
    }
    public async Task<ExecucaoCompra> SalvarExecucaoAsync(ExecucaoCompra execution, CancellationToken ct = default)
    {
        var entity = new ExecucaoCompraEntity
        {
            DataReferencia = execution.DataReferencia,
            DataExecucao = execution.DataExecucao,
            TotalConsolidado = execution.TotalConsolidado,
            TotalClientes = execution.TotalClientes
        };
        foreach (var o in execution.Ordens)
        {
            entity.Ordens.Add(new OrdemCompraEntity
            {
                Ticker = o.Ticker,
                QuantidadeTotal = o.QuantidadeTotal,
                PrecoUnitario = o.PrecoUnitario,
                ValorTotal = o.ValorTotal,
                DetalhesJson = JsonSerializer.Serialize(o.Detalhes)
            });
        }
        foreach (var d in execution.Distribuicoes)
        {
            entity.Distribuicoes.Add(new DistribuicaoEntity
            {
                ClienteId = d.ClienteId,
                Nome = d.Nome,
                Cpf = d.Cpf,
                ValorAporte = d.ValorAporte,
                AtivosJson = JsonSerializer.Serialize(d.Ativos)
            });
        }
        _db.ExecucoesCompra.Add(entity);
        await _db.SaveChangesAsync(ct);
        execution.Id = entity.Id;
        return execution;
    }
}
