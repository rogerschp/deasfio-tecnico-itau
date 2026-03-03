using Cotacao.Application.Contracts;
using Cotacao.Infrastructure.Parser;
using Cotacao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Cotacao.Infrastructure;

public static class ServiceCollectionExtensions
{
    private const string DefaultMySqlVersion = "8.0.0";

    public static IServiceCollection AddCotacaoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Cotacao")
            ?? throw new InvalidOperationException("ConnectionStrings:Cotacao is required.");
        var serverVersion = ServerVersion.Parse(
            configuration["Cotacao:MySqlVersion"] ?? DefaultMySqlVersion);
        return services.AddCotacaoInfrastructure(connectionString, serverVersion);
    }

    public static IServiceCollection AddCotacaoInfrastructure(
        this IServiceCollection services,
        string connectionString,
        ServerVersion serverVersion)
    {
        services.AddDbContext<CotacaoDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        services.AddScoped<ICotacaoRepository, CotacaoRepository>();
        services.AddSingleton<ICotahistParser, CotahistParser>();
        return services;
    }
}
