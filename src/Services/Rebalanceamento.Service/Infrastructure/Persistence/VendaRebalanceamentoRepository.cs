using Microsoft.EntityFrameworkCore;
using Rebalanceamento.Service.Application.Ports;
namespace Rebalanceamento.Service.Infrastructure.Persistence;
public class VendaRebalanceamentoRepository : IVendaRebalanceamentoRepository
{
    private readonly RebalanceamentoDbContext _db;
    public VendaRebalanceamentoRepository(RebalanceamentoDbContext db) => _db = db;
    public async Task RegistrarVendaAsync(long clienteId, string cpf, string ticker, int quantidade, decimal precoVenda, decimal precoMedio, decimal lucro, DateTime data, CancellationToken ct = default)
    {
        _db.VendasRebalanceamento.Add(new VendaRebalanceamentoEntity
        {
            ClienteId = clienteId,
            Cpf = cpf,
            Ticker = ticker,
            Quantidade = quantidade,
            PrecoVenda = precoVenda,
            PrecoMedio = precoMedio,
            Lucro = lucro,
            DataExecucao = data
        });
        await _db.SaveChangesAsync(ct);
    }
    public async Task<(decimal TotalVendasMes, decimal LucroLiquidoMes)> GetTotalVendasELucroClienteNoMesAsync(long clienteId, int ano, int mes, CancellationToken ct = default)
    {
        var inicio = new DateTime(ano, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);
        var vendas = await _db.VendasRebalanceamento
            .Where(v => v.ClienteId == clienteId && v.DataExecucao >= inicio && v.DataExecucao < fim)
            .ToListAsync(ct);
        decimal totalVendas = 0, lucroLiquido = 0;
        for (var i = 0; i < vendas.Count; i++)
        {
            var v = vendas[i];
            totalVendas += v.Quantidade * v.PrecoVenda;
            lucroLiquido += v.Lucro;
        }
        return (totalVendas, lucroLiquido);
    }
}
