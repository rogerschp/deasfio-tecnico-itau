using Cotacao.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cotacao.Infrastructure.Persistence;

/// <summary>
/// DbContext para o bounded context de Cotações B3.
/// Usado por EF Core para migrations e schema; leituras/escritas em massa usam Dapper/raw SQL.
/// </summary>
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
