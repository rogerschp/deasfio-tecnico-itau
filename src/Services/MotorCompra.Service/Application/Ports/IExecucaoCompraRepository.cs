using MotorCompra.Service.Application.Entities;

namespace MotorCompra.Service.Application.Ports;

/// <summary>
/// Porta de persistência de execuções de compra e distribuições.
/// </summary>
public interface IExecucaoCompraRepository
{
    Task<ExecucaoCompra> SalvarExecucaoAsync(ExecucaoCompra execucao, CancellationToken ct = default);
    Task<bool> JaExecutouNaDataAsync(DateOnly dataReferencia, CancellationToken ct = default);
}
