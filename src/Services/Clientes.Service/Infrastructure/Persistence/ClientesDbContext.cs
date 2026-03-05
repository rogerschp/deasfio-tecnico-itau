using Microsoft.EntityFrameworkCore;
namespace Clientes.Service.Infrastructure.Persistence;
public class ClientesDbContext : DbContext
{
    public ClientesDbContext(DbContextOptions<ClientesDbContext> options) : base(options) { }
    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();
    public DbSet<ContaGraficaEntity> ContasGraficas => Set<ContaGraficaEntity>();
    public DbSet<CustodiaFilhoteEntity> CustodiasFilhote => Set<CustodiaFilhoteEntity>();
    public DbSet<AporteEntity> Aportes => Set<AporteEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClienteEntity>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.Cpf).HasMaxLength(11).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.ValorMensal).HasPrecision(18, 2);
            e.HasIndex(x => x.Cpf).IsUnique();
        });
        modelBuilder.Entity<ContaGraficaEntity>(e =>
        {
            e.ToTable("ContasGraficas");
            e.HasKey(x => x.Id);
            e.Property(x => x.NumeroConta).HasMaxLength(20).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.NumeroConta).IsUnique();
        });
        modelBuilder.Entity<CustodiaFilhoteEntity>(e =>
        {
            e.ToTable("CustodiasFilhote");
            e.HasKey(x => x.Id);
            e.Property(x => x.Ticker).HasMaxLength(12).IsRequired();
            e.Property(x => x.PrecoMedio).HasPrecision(18, 4);
            e.HasIndex(x => new { x.ContaGraficaId, x.Ticker }).IsUnique();
        });
        modelBuilder.Entity<AporteEntity>(e =>
        {
            e.ToTable("Aportes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Valor).HasPrecision(18, 2);
            e.HasIndex(x => x.ClienteId);
        });
    }
}
