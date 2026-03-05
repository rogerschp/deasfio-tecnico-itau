using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace Shared.Kafka;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaEventoIR(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(KafkaOptions.SectionName);
        var options = new KafkaOptions
        {
            BootstrapServers = section["BootstrapServers"] ?? "localhost:9092",
            TopicoIR = section["TopicoIR"] ?? "ir-dedo-duro"
        };
        services.AddSingleton<IOptions<KafkaOptions>>(Options.Create(options));
        services.AddSingleton<IEventoIRPublisher, KafkaEventoIRPublisher>();
        return services;
    }
}
