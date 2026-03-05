using Microsoft.AspNetCore.Mvc;
using Rebalanceamento.Service.Application.Exceptions;
using Rebalanceamento.Service.Application.Ports;
using Rebalanceamento.Service.Application.Services;
namespace Rebalanceamento.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RebalanceamentoController : ControllerBase
{
    private readonly IExecutarRebalanceamentoService _service;
    public RebalanceamentoController(IExecutarRebalanceamentoService service) => _service = service;

    [HttpPost("por-mudanca-cesta")]
    [ProducesResponseType(typeof(ResultadoRebalanceamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultadoRebalanceamentoDto>> PorMudancaCesta([FromBody] PorMudancaCestaRequest request, CancellationToken ct = default)
    {
        if (request?.CestaAnterior == null || request.CestaNova == null ||
            request.CestaAnterior.Count == 0 || request.CestaNova.Count == 0)
            return BadRequest(new { codigo = "CESTA_INVALIDA", erro = "Informe cesta anterior e nova com itens." });
        try
        {
            var previousBasket = request.CestaAnterior.Select(i => new ItemCestaDto(i.Ticker, i.Percentual)).ToList();
            var newBasket = request.CestaNova.Select(i => new ItemCestaDto(i.Ticker, i.Percentual)).ToList();
            var result = await _service.ExecutarPorMudancaCestaAsync(previousBasket, newBasket, ct);
            if (result is null)
                return BadRequest(new { codigo = "EXECUCAO_INVALIDA", erro = "Não foi possível executar o rebalanceamento." });
            return Ok(result);
        }
        catch (KafkaIndisponivelException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { codigo = "KAFKA_INDISPONIVEL", erro = "Falha ao publicar evento no Kafka." });
        }
    }

    [HttpPost("por-desvio")]
    [ProducesResponseType(typeof(ResultadoRebalanceamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultadoRebalanceamentoDto>> PorDesvio(
        [FromQuery] decimal thresholdPercentage = RebalanceamentoConstants.LimiarDesvioPadrao,
        CancellationToken ct = default)
    {
        if (thresholdPercentage <= 0 || thresholdPercentage > RebalanceamentoConstants.LimiarPercentualMaximo)
            return BadRequest(new { codigo = "LIMIAR_INVALIDO", erro = "Limiar deve estar entre 0 e 100." });
        try
        {
            var result = await _service.ExecutarPorDesvioAsync(thresholdPercentage, ct);
            if (result is null)
                return BadRequest(new { codigo = "EXECUCAO_INVALIDA", erro = "Não foi possível executar o rebalanceamento." });
            return Ok(result);
        }
        catch (KafkaIndisponivelException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { codigo = "KAFKA_INDISPONIVEL", erro = "Falha ao publicar evento no Kafka." });
        }
    }
}
public record PorMudancaCestaRequest(
    IReadOnlyList<ItemCestaRequest> CestaAnterior,
    IReadOnlyList<ItemCestaRequest> CestaNova);
public record ItemCestaRequest(string Ticker, decimal Percentual);
