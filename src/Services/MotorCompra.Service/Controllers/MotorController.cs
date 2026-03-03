using Microsoft.AspNetCore.Mvc;
using MotorCompra.Service.Application.Entities;
using MotorCompra.Service.Application.Services;

namespace MotorCompra.Service.Controllers;

/// <summary>
/// API do Motor de Compra Programada: execução manual (testes) e consulta.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MotorController : ControllerBase
{
    private readonly IExecutarCompraProgramadaService _executarService;

    public MotorController(IExecutarCompraProgramadaService executarService)
    {
        _executarService = executarService;
    }

    /// <summary>
    /// Executa a compra programada para uma data de referência (dia 5, 15 ou 25).
    /// Usado pelo Worker nos dias configurados ou manualmente para testes.
    /// </summary>
    /// <param name="request">Data de referência (ex: 2026-02-05).</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <response code="200">Execução realizada; retorna resultado.</response>
    /// <response code="204">Nada a executar (já executado na data, sem cesta/clientes/cotação).</response>
    [HttpPost("executar-compra")]
    [ProducesResponseType(typeof(ExecucaoCompraResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<ExecucaoCompraResultDto>> ExecutarCompra(
        [FromBody] ExecutarCompraRequest request,
        CancellationToken ct)
    {
        var dataRef = request?.DataReferencia ?? DateOnly.FromDateTime(DateTime.Today);
        var execucao = await _executarService.ExecutarAsync(dataRef, ct);
        if (execucao is null)
            return NoContent();
        return Ok(ExecucaoCompraResultDtoMapper.From(execucao));
    }
}

public record ExecutarCompraRequest(DateOnly? DataReferencia);

public record ExecucaoCompraResultDto(
    DateTime DataExecucao,
    int TotalClientes,
    decimal TotalConsolidado,
    IReadOnlyList<OrdemCompraResultDto> OrdensCompra,
    IReadOnlyList<DistribuicaoResultDto> Distribuicoes);

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
    public static ExecucaoCompraResultDto From(ExecucaoCompra e) => new(
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
            d.Ativos.Select(a => new AtivoDistribuidoResultDto(a.Ticker, a.Quantidade)).ToList())).ToList());
}
