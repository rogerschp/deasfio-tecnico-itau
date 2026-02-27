namespace Shared.Kafka;

public class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TopicoIR { get; set; } = "ir-dedo-duro";
}
