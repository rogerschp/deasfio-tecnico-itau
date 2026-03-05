using Cotacao.Domain;
using Microsoft.EntityFrameworkCore;
namespace Cotacao.Infrastructure.Persistence;

public class CotacaoDbContext : DbContext
{
    public CotacaoDbContext(DbContextOptions<CotacaoDbContext> options)
        : base(options)
    {
    }
    public DbSet<CotacaoB3> Cotacoes => Set<CotacaoB3>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CotacaoDbContext).Assembly);
    }
}
