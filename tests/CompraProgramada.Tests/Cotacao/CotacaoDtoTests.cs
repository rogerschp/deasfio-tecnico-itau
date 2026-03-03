using Cotacao.Application.DTOs;

namespace CompraProgramada.Tests.Cotacao;

public class CotacaoDtoTests
{
    [Fact]
    public void CotacaoFechamentoDto_Deve_Refletir_Valores_Informados()
    {
        var dto = new CotacaoFechamentoDto("PETR4", new DateOnly(2026, 2, 25), 35.80m);
        Assert.Equal("PETR4", dto.Ticker);
        Assert.Equal(new DateOnly(2026, 2, 25), dto.DataPregao);
        Assert.Equal(35.80m, dto.PrecoFechamento);
    }

    [Fact]
    public void ImportacaoResultDto_Deve_Refletir_Resultado_Da_Importacao()
    {
        var dto = new ImportacaoResultDto(new DateOnly(2026, 2, 25), 1500, false);
        Assert.Equal(new DateOnly(2026, 2, 25), dto.DataPregao);
        Assert.Equal(1500, dto.RegistrosInseridos);
        Assert.False(dto.PregaoJaExistia);
    }

    [Fact]
    public void ImportacaoResultDto_PregaoJaExistia_Retorna_Zero_Inseridos()
    {
        var dto = new ImportacaoResultDto(new DateOnly(2026, 2, 25), 0, true);
        Assert.True(dto.PregaoJaExistia);
        Assert.Equal(0, dto.RegistrosInseridos);
    }
}
