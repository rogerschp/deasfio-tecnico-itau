using Shared.Contracts.Eventos;

namespace Shared.Kafka;

/// <summary>
/// Publica eventos de IR (dedo-duro e IR venda) no tópico Kafka.
/// </summary>
public interface IEventoIRPublisher
{
    Task PublicarDedoDuroAsync(EventoIRDedoDuro evento, CancellationToken ct = default);
    Task PublicarIRVendaAsync(EventoIRVenda evento, CancellationToken ct = default);
}
