using MotorCompra.Service.Application.Entities;
namespace MotorCompra.Service.Application.Services;

public interface IExecutarCompraProgramadaService
{

    Task<ExecucaoCompra?> ExecutarAsync(DateOnly referenceDate, CancellationToken ct = default);
}
