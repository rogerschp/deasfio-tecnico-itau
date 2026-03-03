using MotorCompra.Service.Application.Entities;

namespace MotorCompra.Service.Application.Services;

/// <summary>
/// Caso de uso: executar a compra programada para uma data de referência (dia 5, 15 ou 25).
/// </summary>
public interface IExecutarCompraProgramadaService
{
    /// <summary>
    /// Executa o fluxo completo: agrupa clientes, obtém cesta e cotações, calcula compra,
    /// registra ordens, distribui para filhotes, atualiza custódia master e publica IR no Kafka.
    /// </summary>
    Task<ExecucaoCompra?> ExecutarAsync(DateOnly dataReferencia, CancellationToken ct = default);
}
