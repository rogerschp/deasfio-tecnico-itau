using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MotorCompra.Service.Application.Entities;
using MotorCompra.Service.Application.Ports;

namespace MotorCompra.Service.Infrastructure.Persistence;

public class ExecucaoCompraRepository : IExecucaoCompraRepository
{
    private readonly MotorDbContext _db;

    public ExecucaoCompraRepository(MotorDbContext db) => _db = db;

    public async Task<bool> JaExecutouNaDataAsync(DateOnly dataReferencia, CancellationToken ct = default)
    {
        return await _db.ExecucoesCompra.AnyAsync(e => e.DataReferencia == dataReferencia, ct);
    }

    public async Task<ExecucaoCompra> SalvarExecucaoAsync(ExecucaoCompra execucao, CancellationToken ct = default)
    {
        var entity = new ExecucaoCompraEntity
        {
            DataReferencia = execucao.DataReferencia,
            DataExecucao = execucao.DataExecucao,
            TotalConsolidado = execucao.TotalConsolidado,
            TotalClientes = execucao.TotalClientes
        };
        foreach (var o in execucao.Ordens)
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
        foreach (var d in execucao.Distribuicoes)
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
        execucao.Id = entity.Id;
        return execucao;
    }
}
