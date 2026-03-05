using Microsoft.AspNetCore.Mvc;
using MotorCompra.Service.Application.Entities;
using MotorCompra.Service.Application.Exceptions;
using MotorCompra.Service.Application.Ports;
using MotorCompra.Service.Application.Services;
namespace MotorCompra.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MotorController : ControllerBase
{
    private readonly IExecutarCompraProgramadaService _executarService;
    private readonly ICustodiaMasterRepository _custodiaMasterRepo;
    private readonly ICotacaoFechamentoClient? _cotacaoClient;
    public MotorController(IExecutarCompraProgramadaService executarService, ICustodiaMasterRepository custodiaMasterRepo, ICotacaoFechamentoClient? cotacaoClient = null)
    {
        _executarService = executarService;
        _custodiaMasterRepo = custodiaMasterRepo;
        _cotacaoClient = cotacaoClient;
    }

    [HttpPost("executar-compra")]
    [ProducesResponseType(typeof(ExecucaoCompraResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExecucaoCompraResultDto>> ExecutarCompra(
        [FromBody] ExecutarCompraRequest request,
        CancellationToken ct)
    {
        var referenceDate = request?.DataReferencia ?? DateOnly.FromDateTime(DateTime.Today);
        try
        {
            var result = await _executarService.ExecutarAsync(referenceDate, ct);
            if (result is null)
                return NoContent();
            return Ok(ExecucaoCompraResultDtoMapper.From(result));
        }
        catch (CompraJaExecutadaException ex)
        {
            return Conflict(new { codigo = "COMPRA_JA_EXECUTADA", dataReferencia = ex.DataReferencia.ToString("yyyy-MM-dd"), erro = ex.Message });
        }
        catch (KafkaIndisponivelException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { codigo = "KAFKA_INDISPONIVEL", erro = "Falha ao publicar evento no Kafka." });
        }
    }

    [HttpGet("custodia-master")]
    [ProducesResponseType(typeof(CustodiaMasterResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustodiaMasterResponseDto>> GetCustodiaMaster(CancellationToken ct)
    {
        var residuals = await _custodiaMasterRepo.GetTodosResiduosAsync(ct);
        if (residuals.Count == 0)
            return Ok(new CustodiaMasterResponseDto(new ContaMasterDto(1, "MST-000001", "MASTER"), new List<CustodiaMasterItemDto>(), 0));
        var tickerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < residuals.Count; i++)
            tickerSet.Add(residuals[i].Ticker);
        var tickers = new List<string>(tickerSet);
        var priceByTicker = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        if (_cotacaoClient != null && tickers.Count > 0)
        {
            var quotes = await _cotacaoClient.GetFechamentosAsync(tickers, ct);
            foreach (var c in quotes)
                priceByTicker[c.Ticker] = c.PrecoFechamento;
        }
        decimal totalResidualValue = 0;
        var custody = new List<CustodiaMasterItemDto>();
        foreach (var (ticker, quantity) in residuals)
        {
            var currentValue = priceByTicker.GetValueOrDefault(ticker, 0);
            totalResidualValue += quantity * currentValue;
            custody.Add(new CustodiaMasterItemDto(ticker, quantity, currentValue, currentValue, "Resíduo distribuição"));
        }
        return Ok(new CustodiaMasterResponseDto(new ContaMasterDto(1, "MST-000001", "MASTER"), custody, totalResidualValue));
    }
}
public record ContaMasterDto(long Id, string NumeroConta, string Tipo);
public record CustodiaMasterItemDto(string Ticker, int Quantidade, decimal PrecoMedio, decimal ValorAtual, string Origem);
public record CustodiaMasterResponseDto(ContaMasterDto ContaMaster, IReadOnlyList<CustodiaMasterItemDto> Custodia, decimal ValorTotalResiduo);
public record ExecutarCompraRequest(DateOnly? DataReferencia);
public record ExecucaoCompraResultDto(
    DateTime DataExecucao,
    int TotalClientes,
    decimal TotalConsolidado,
    IReadOnlyList<OrdemCompraResultDto> OrdensCompra,
    IReadOnlyList<DistribuicaoResultDto> Distribuicoes,
    IReadOnlyList<ResiduoCustMasterDto> ResiduosCustMaster,
    int EventosIRPublicados,
    string Mensagem);
public record ResiduoCustMasterDto(string Ticker, int Quantidade);
public record OrdemCompraResultDto(
    string Ticker,
    int QuantidadeTotal,
    decimal PrecoUnitario,
    decimal ValorTotal,
    IReadOnlyList<DetalheOrdemResultDto> Detalhes);
public record DetalheOrdemResultDto(string Tipo, string Ticker, int Quantidade);
public record DistribuicaoResultDto(
    long ClienteId,
    string Nome,
    decimal ValorAporte,
    IReadOnlyList<AtivoDistribuidoResultDto> Ativos);
public record AtivoDistribuidoResultDto(string Ticker, int Quantidade);
public static class ExecucaoCompraResultDtoMapper
{
    public static ExecucaoCompraResultDto From(ExecucaoCompraComResiduos r)
    {
        var e = r.Execucao;
        var eventosIR = e.Distribuicoes.Sum(d => d.Ativos.Count);
        var residuos = r.Residuos.Where(x => x.Quantidade > 0).Select(x => new ResiduoCustMasterDto(x.Ticker, x.Quantidade)).ToList();
        return new ExecucaoCompraResultDto(
            DataExecucao: e.DataExecucao,
            TotalClientes: e.TotalClientes,
            TotalConsolidado: e.TotalConsolidado,
            OrdensCompra: e.Ordens.Select(o => new OrdemCompraResultDto(
                o.Ticker,
                o.QuantidadeTotal,
                o.PrecoUnitario,
                o.ValorTotal,
                o.Detalhes.Select(d => new DetalheOrdemResultDto(d.Tipo, d.Ticker, d.Quantidade)).ToList())).ToList(),
            Distribuicoes: e.Distribuicoes.Select(d => new DistribuicaoResultDto(
                d.ClienteId,
                d.Nome,
                d.ValorAporte,
                d.Ativos.Select(a => new AtivoDistribuidoResultDto(a.Ticker, a.Quantidade)).ToList())).ToList(),
            ResiduosCustMaster: residuos,
            EventosIRPublicados: eventosIR,
            Mensagem: $"Compra programada executada com sucesso para {e.TotalClientes} cliente(s).");
    }
}
