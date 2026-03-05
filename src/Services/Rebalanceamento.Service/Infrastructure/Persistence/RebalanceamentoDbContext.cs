using Microsoft.EntityFrameworkCore;
namespace Rebalanceamento.Service.Infrastructure.Persistence;
public class RebalanceamentoDbContext : DbContext
{
    public RebalanceamentoDbContext(DbContextOptions<RebalanceamentoDbContext> options) : base(options) { }
    public DbSet<VendaRebalanceamentoEntity> VendasRebalanceamento => Set<VendaRebalanceamentoEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VendaRebalanceamentoEntity>(e =>
        {
            e.ToTable("vendas_rebalanceamento");
            e.HasKey(x => x.Id);
            e.Property(x => x.ClienteId).IsRequired();
            e.Property(x => x.Cpf).HasMaxLength(20).IsRequired();
            e.Property(x => x.Ticker).HasMaxLength(20).IsRequired();
            e.Property(x => x.Quantidade).IsRequired();
            e.Property(x => x.PrecoVenda).HasPrecision(18, 4).IsRequired();
            e.Property(x => x.PrecoMedio).HasPrecision(18, 4).IsRequired();
            e.Property(x => x.Lucro).HasPrecision(18, 4).IsRequired();
            e.Property(x => x.DataExecucao).IsRequired();
            e.HasIndex(x => new { x.ClienteId, x.DataExecucao });
        });
    }
}
