using System.Data;
using Cotacao.Application.Contracts;
using Cotacao.Domain;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Cotacao.Infrastructure.Persistence;

/// <summary>
/// Repositório de cotações: usa Dapper/raw SQL para consultas e bulk insert (performance).
/// </summary>
public sealed class CotacaoRepository : ICotacaoRepository
{
    private readonly CotacaoDbContext _context;
    private const int BulkBatchSize = 500;

    public CotacaoRepository(CotacaoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<CotacaoB3?> GetFechamentoUltimoPregaoAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        const string sql = """
            SELECT Id, DataPregao, Ticker, PrecoAbertura, PrecoFechamento, PrecoMaximo, PrecoMinimo
            FROM Cotacoes
            WHERE Ticker = @Ticker
            ORDER BY DataPregao DESC
            LIMIT 1
            """;

        var cmd = new CommandDefinition(sql, new { Ticker = ticker }, cancellationToken: cancellationToken);
        return await conn.QuerySingleOrDefaultAsync<CotacaoB3>(cmd);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CotacaoB3>> GetFechamentosUltimoPregaoPorTickersAsync(
        IReadOnlyList<string> tickers,
        CancellationToken cancellationToken = default)
    {
        if (tickers.Count == 0)
            return Array.Empty<CotacaoB3>();

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        // Último pregão = MAX(DataPregao). Buscamos cotações desse dia para os tickers solicitados.
        const string sql = """
            SELECT c.Id, c.DataPregao, c.Ticker, c.PrecoAbertura, c.PrecoFechamento, c.PrecoMaximo, c.PrecoMinimo
            FROM Cotacoes c
            INNER JOIN (
                SELECT Ticker, MAX(DataPregao) AS DataPregao
                FROM Cotacoes
                WHERE Ticker IN @Tickers
                GROUP BY Ticker
            ) last ON c.Ticker = last.Ticker AND c.DataPregao = last.DataPregao
            WHERE c.Ticker IN @Tickers
            """;

        var cmd = new CommandDefinition(sql, new { Tickers = tickers }, cancellationToken: cancellationToken);
        var list = await conn.QueryAsync<CotacaoB3>(cmd);
        return list.ToList();
    }

    /// <inheritdoc />
    public async Task<int> BulkInsertAsync(IEnumerable<CotacaoB3> cotacoes, CancellationToken cancellationToken = default)
    {
        var list = cotacoes.ToList();
        if (list.Count == 0)
            return 0;

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        var total = 0;
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var batch in list.Chunk(BulkBatchSize))
            {
                var (sql, param) = BuildBulkInsert(batch);
                await conn.ExecuteAsync(
                    new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));
                total += batch.Length;
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return total;
    }

    private static (string Sql, object Param) BuildBulkInsert(CotacaoB3[] batch)
    {
        var values = new List<string>(batch.Length);
        var param = new DynamicParameters();
        for (var i = 0; i < batch.Length; i++)
        {
            var p = batch[i];
            var prefix = $"p{i}_";
            values.Add($"(@{prefix}dp, @{prefix}t, @{prefix}pa, @{prefix}pf, @{prefix}pmx, @{prefix}pmn)");
            param.Add(prefix + "dp", p.DataPregao);
            param.Add(prefix + "t", p.Ticker);
            param.Add(prefix + "pa", p.PrecoAbertura);
            param.Add(prefix + "pf", p.PrecoFechamento);
            param.Add(prefix + "pmx", p.PrecoMaximo);
            param.Add(prefix + "pmn", p.PrecoMinimo);
        }

        var sql = """
            INSERT INTO Cotacoes (DataPregao, Ticker, PrecoAbertura, PrecoFechamento, PrecoMaximo, PrecoMinimo)
            VALUES
            """ + string.Join(",\n", values);
        return (sql, param);
    }

    /// <inheritdoc />
    public async Task<bool> ExistePregaoAsync(DateOnly dataPregao, CancellationToken cancellationToken = default)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        const string sql = "SELECT 1 FROM Cotacoes WHERE DataPregao = @DataPregao LIMIT 1";
        var cmd = new CommandDefinition(sql, new { DataPregao = dataPregao }, cancellationToken: cancellationToken);
        return await conn.ExecuteScalarAsync<int?>(cmd) is 1;
    }
}
