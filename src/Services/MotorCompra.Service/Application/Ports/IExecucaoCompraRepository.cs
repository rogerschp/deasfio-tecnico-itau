using MotorCompra.Service.Application.Entities;
namespace MotorCompra.Service.Application.Ports;

public interface IExecucaoCompraRepository
{
    Task<ExecucaoCompra> SalvarExecucaoAsync(ExecucaoCompra execucao, CancellationToken ct = default);
    Task<bool> JaExecutouNaDataAsync(DateOnly referenceDate, CancellationToken ct = default);
}
