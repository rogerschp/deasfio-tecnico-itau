using MotorCompra.Service.Application.Entities;
namespace MotorCompra.Service.Application.Services;

public interface IExecutarCompraProgramadaService
{
    Task<ExecucaoCompraComResiduos?> ExecutarAsync(DateOnly referenceDate, CancellationToken ct = default);
}

public record ExecucaoCompraComResiduos(ExecucaoCompra Execucao, IReadOnlyList<(string Ticker, int Quantidade)> Residuos);
