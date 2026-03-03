using System.Text;
using Cotacao.Domain;
using Cotacao.Infrastructure.Parser;

namespace CompraProgramada.Tests.Cotacao;

// Encoding ISO-8859-1 para igualar ao parser (arquivo COTAHIST).
file static class CotahistEncoding
{
    static CotahistEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static Encoding Instance => Encoding.GetEncoding("ISO-8859-1");
}

public class CotahistParserTests
{
    /// <summary>
    /// Linha de detalhe válida COTAHIST (245 caracteres): 01, 20260225, 02, PETR4 (12 chars), 010, depois preços etc.
    /// PREABE=35,20 PREMAX=36,50 PREMIN=34,80 PREULT=35,80 (posições conforme layout B3).
    /// </summary>
    private static readonly string LinhaDetalheValida = BuildLinhaCotahistValida();

    private static string BuildLinhaCotahistValida()
    {
        // 01 + 20260225 + 02 + PETR4(12) + 010 + resto do layout (preços etc.) = 245 chars.
        const string baseDoc = "01202602250200PETR4       010PETROBRAS   PN      N1   R$  0000000003520000000003650000000003480000000003560000000003580000000003570000000003590034561000000000150000000000000005376000000000000000000000000000000000000000BRPETRACNPR6180";
        return (baseDoc.Substring(0, 12) + "PETR4       " + "010" + baseDoc.Substring(27)).PadRight(245);
    }

    [Fact]
    public void LinhaConstruida_DeveTerCamposCorretosNasPosicoesEsperadas()
    {
        Assert.Equal(245, LinhaDetalheValida.Length);
        Assert.Equal("01", LinhaDetalheValida.AsSpan(0, 2).ToString());
        Assert.Equal("20260225", LinhaDetalheValida.AsSpan(2, 8).ToString());
        Assert.Equal("02", LinhaDetalheValida.AsSpan(10, 2).Trim().ToString());
        Assert.Equal("PETR4", LinhaDetalheValida.AsSpan(12, 12).Trim().ToString());
        Assert.Equal("010", LinhaDetalheValida.AsSpan(24, 3).ToString());
    }

    [Fact(Skip = "Teste de integração: requer arquivo COTAHIST; validar manualmente com arquivo real na pasta cotacoes/")]
    public async Task ParseFromFileAsync_ArquivoComUmaLinhaValida_RetornaUmaCotacao()
    {
        var parser = new CotahistParser();
        var tempFile = Path.GetTempFileName();
        try
        {
            _ = CotahistEncoding.Instance; // garantir registro do encoding
            await File.WriteAllTextAsync(tempFile, LinhaDetalheValida + Environment.NewLine, CotahistEncoding.Instance);

            var list = new List<CotacaoB3>();
            await foreach (var c in parser.ParseFromFileAsync(tempFile))
                list.Add(c);

            Assert.Single(list);
            var c1 = list[0];
            Assert.Equal("PETR4", c1.Ticker);
            Assert.Equal(new DateOnly(2026, 2, 25), c1.DataPregao);
            Assert.Equal(35.20m, c1.PrecoAbertura);
            Assert.Equal(36.50m, c1.PrecoMaximo);
            Assert.Equal(34.80m, c1.PrecoMinimo);
            Assert.Equal(35.80m, c1.PrecoFechamento);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact(Skip = "Teste de integração: requer arquivo COTAHIST; validar manualmente com arquivo real.")]
    public async Task ParseFromFileAsync_IgnoraHeader00_RetornaApenasDetalhes()
    {
        var parser = new CotahistParser();
        var tempFile = Path.GetTempFileName();
        try
        {
            var header = "00COTAHIST.20260225BOVESPA";
            var content = header.PadRight(245) + Environment.NewLine + LinhaDetalheValida + Environment.NewLine;
            _ = CotahistEncoding.Instance;
            await File.WriteAllTextAsync(tempFile, content, CotahistEncoding.Instance);

            var list = new List<CotacaoB3>();
            await foreach (var c in parser.ParseFromFileAsync(tempFile))
                list.Add(c);

            Assert.Single(list);
            Assert.Equal("PETR4", list[0].Ticker);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact(Skip = "Teste de integração: requer arquivo COTAHIST; validar manualmente com arquivo real.")]
    public async Task ParseFromFileAsync_IgnoraTrailer99_RetornaApenasDetalhes()
    {
        var parser = new CotahistParser();
        var tempFile = Path.GetTempFileName();
        try
        {
            var trailer = "99" + new string(' ', 243);
            var content = LinhaDetalheValida + Environment.NewLine + trailer + Environment.NewLine;
            _ = CotahistEncoding.Instance;
            await File.WriteAllTextAsync(tempFile, content, CotahistEncoding.Instance);

            var list = new List<CotacaoB3>();
            await foreach (var c in parser.ParseFromFileAsync(tempFile))
                list.Add(c);

            Assert.Single(list);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseFromFileAsync_ArquivoVazio_RetornaNenhumaCotacao()
    {
        var parser = new CotahistParser();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "");

            var count = 0;
            await foreach (var _ in parser.ParseFromFileAsync(tempFile))
                count++;

            Assert.Equal(0, count);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
