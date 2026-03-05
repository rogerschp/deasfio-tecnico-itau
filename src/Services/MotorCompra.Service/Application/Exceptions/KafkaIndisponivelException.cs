namespace MotorCompra.Service.Application.Exceptions;

public class KafkaIndisponivelException : Exception
{
    public KafkaIndisponivelException(string? message = null, Exception? inner = null)
        : base(message ?? "Kafka indisponível.", inner) { }
}
