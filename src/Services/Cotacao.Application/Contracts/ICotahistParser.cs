using Cotacao.Domain;
namespace Cotacao.Application.Contracts;

public interface ICotahistParser
{

    IAsyncEnumerable<CotacaoB3> ParseFromFileAsync(string filePath, CancellationToken cancellationToken = default);
}
