using System.Text;
using Cotacao.Application.Contracts;
using Cotacao.Domain;
namespace Cotacao.Infrastructure.Parser;

public sealed class CotahistParser : ICotahistParser
{
    private const decimal FatorDecimalPreco = 100m;
    private const int LineLength = 245;
    private const string TipRegDetalhe = "01";
    private static readonly HashSet<string> CodBdiAllowed = ["02", "96"];
    private static readonly HashSet<int> TpMercAllowed = [10, 20];
    static CotahistParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public async IAsyncEnumerable<CotacaoB3> ParseFromFileAsync(string filePath, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var encoding = Encoding.GetEncoding("ISO-8859-1");
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: false);
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (line.Length < LineLength)
                continue;
            var recordType = line.AsSpan(0, 2).ToString();
            if (recordType != TipRegDetalhe)
                continue;
            if (!int.TryParse(line.AsSpan(24, 3), out var tpMerc) || !TpMercAllowed.Contains(tpMerc))
                continue;
            var bdiCode = line.AsSpan(10, 2).Trim().ToString();
            if (!CodBdiAllowed.Contains(bdiCode))
                continue;
            if (!DateOnly.TryParseExact(line.AsSpan(2, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var tradingDate))
                continue;
            var ticker = line.AsSpan(12, 12).Trim().ToString();
            if (string.IsNullOrEmpty(ticker))
                continue;
            yield return new CotacaoB3
            {
                DataPregao = tradingDate,
                Ticker = ticker,
                PrecoAbertura = ParsePreco(line.AsSpan(56, 13)),
                PrecoMaximo = ParsePreco(line.AsSpan(69, 13)),
                PrecoMinimo = ParsePreco(line.AsSpan(82, 13)),
                PrecoFechamento = ParsePreco(line.AsSpan(108, 13))
            };
        }
    }
    private static decimal ParsePreco(ReadOnlySpan<char> value)
    {
        if (long.TryParse(value.Trim(), System.Globalization.NumberStyles.None, null, out var v))
            return v / FatorDecimalPreco;
        return 0m;
    }
}
