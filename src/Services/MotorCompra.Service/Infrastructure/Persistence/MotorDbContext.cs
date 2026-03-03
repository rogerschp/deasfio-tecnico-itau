using Microsoft.EntityFrameworkCore;

namespace MotorCompra.Service.Infrastructure.Persistence;

public class MotorDbContext : DbContext
{
    public MotorDbContext(DbContextOptions<MotorDbContext> options) : base(options) { }

    public DbSet<ExecucaoCompraEntity> ExecucoesCompra => Set<ExecucaoCompraEntity>();
    public DbSet<OrdemCompraEntity> OrdensCompra => Set<OrdemCompraEntity>();
    public DbSet<DistribuicaoEntity> Distribuicoes => Set<DistribuicaoEntity>();
    public DbSet<CustodiaMasterEntity> CustodiaMaster => Set<CustodiaMasterEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExecucaoCompraEntity>(e =>
        {
            e.ToTable("ExecucoesCompra");
            e.HasKey(x => x.Id);
            e.Property(x => x.DataReferencia).IsRequired();
            e.Property(x => x.DataExecucao).IsRequired();
            e.Property(x => x.TotalConsolidado).HasPrecision(18, 2);
            e.HasIndex(x => x.DataReferencia).IsUnique();
        });

        modelBuilder.Entity<OrdemCompraEntity>(e =>
        {
            e.ToTable("OrdensCompra");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.ExecucaoCompra).WithMany(x => x.Ordens).HasForeignKey(x => x.ExecucaoCompraId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Ticker).HasMaxLength(12).IsRequired();
            e.Property(x => x.PrecoUnitario).HasPrecision(18, 2);
            e.Property(x => x.ValorTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<DistribuicaoEntity>(e =>
        {
            e.ToTable("Distribuicoes");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.ExecucaoCompra).WithMany(x => x.Distribuicoes).HasForeignKey(x => x.ExecucaoCompraId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Nome).HasMaxLength(200);
            e.Property(x => x.Cpf).HasMaxLength(11);
            e.Property(x => x.ValorAporte).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CustodiaMasterEntity>(e =>
        {
            e.ToTable("CustodiaMaster");
            e.HasKey(x => x.Id);
            e.Property(x => x.Ticker).HasMaxLength(12).IsRequired();
            e.HasIndex(x => x.Ticker).IsUnique();
        });
    }
}
