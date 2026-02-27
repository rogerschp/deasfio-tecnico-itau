using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Shared.Contracts.Eventos;

namespace Shared.Kafka;

public class KafkaEventoIRPublisher : IEventoIRPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topico;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public KafkaEventoIRPublisher(IOptions<KafkaOptions> options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.Leader
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
        _topico = options.Value.TopicoIR;
    }

    public async Task PublicarDedoDuroAsync(EventoIRDedoDuro evento, CancellationToken ct = default)
    {
        var key = $"{evento.ClienteId}-{evento.DataOperacao:yyyyMMddHHmmss}";
        var value = JsonSerializer.Serialize(evento, JsonOptions);
        await _producer.ProduceAsync(_topico, new Message<string, string> { Key = key, Value = value }, ct);
    }

    public async Task PublicarIRVendaAsync(EventoIRVenda evento, CancellationToken ct = default)
    {
        var key = $"{evento.ClienteId}-{evento.MesReferencia}";
        var value = JsonSerializer.Serialize(evento, JsonOptions);
        await _producer.ProduceAsync(_topico, new Message<string, string> { Key = key, Value = value }, ct);
    }
}
