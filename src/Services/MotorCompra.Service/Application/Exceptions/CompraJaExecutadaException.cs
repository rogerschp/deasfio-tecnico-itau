namespace MotorCompra.Service.Application.Exceptions;

public class CompraJaExecutadaException : Exception
{
    public DateOnly DataReferencia { get; }
    public CompraJaExecutadaException(DateOnly dataReferencia)
        : base($"Compra programada já executada para a data {dataReferencia:yyyy-MM-dd}.")
    {
        DataReferencia = dataReferencia;
    }
}
