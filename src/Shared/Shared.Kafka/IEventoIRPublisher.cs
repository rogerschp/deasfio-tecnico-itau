using Shared.Contracts.Eventos;
namespace Shared.Kafka;

public interface IEventoIRPublisher
{
    Task PublicarDedoDuroAsync(EventoIRDedoDuro evento, CancellationToken ct = default);
    Task PublicarIRVendaAsync(EventoIRVenda evento, CancellationToken ct = default);
}
