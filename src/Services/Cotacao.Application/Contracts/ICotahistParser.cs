using Cotacao.Domain;

namespace Cotacao.Application.Contracts;

/// <summary>
/// Parser do arquivo COTAHIST da B3 (layout fixo 245 caracteres, encoding ISO-8859-1).
/// Processa apenas registros tipo 01, CODBDI 02/96, TPMERC 010/020.
/// </summary>
public interface ICotahistParser
{
    /// <summary>
    /// Lê e faz parse do arquivo. Retorna apenas registros de detalhe filtrados (lote padrão + fracionário).
    /// </summary>
    IAsyncEnumerable<CotacaoB3> ParseFromFileAsync(string caminhoArquivo, CancellationToken cancellationToken = default);
}
