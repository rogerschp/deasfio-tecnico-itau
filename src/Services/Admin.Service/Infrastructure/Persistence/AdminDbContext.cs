using Microsoft.EntityFrameworkCore;
namespace Admin.Service.Infrastructure.Persistence;
public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }
    public DbSet<CestaEntity> Cestas => Set<CestaEntity>();
    public DbSet<ItemCestaEntity> ItensCesta => Set<ItemCestaEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CestaEntity>(e =>
        {
            e.ToTable("Cestas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Ativa).HasFilter("Ativa = 1");
        });
        modelBuilder.Entity<ItemCestaEntity>(e =>
        {
            e.ToTable("ItensCesta");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Cesta).WithMany(x => x.Itens).HasForeignKey(x => x.CestaId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Ticker).HasMaxLength(12).IsRequired();
            e.Property(x => x.Percentual).HasPrecision(5, 2);
        });
    }
}
