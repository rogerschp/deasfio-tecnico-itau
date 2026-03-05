using Clientes.Service.Application.DTOs;
using Clientes.Service.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clientes.Service.Controllers;

[ApiController]
[Route("api/internal/clientes")]
[Produces("application/json")]
public class ClientesInternalController : ControllerBase
{
    private readonly IClienteAppService _service;

    public ClientesInternalController(IClienteAppService service) => _service = service;

    [HttpPost("distribuicao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarDistribuicao([FromBody] DistribuicaoRequestDto request, CancellationToken ct = default)
    {
        if (request?.Itens == null || request.Itens.Count == 0) return NoContent();
        var items = request.Itens.Select(i => (i.Ticker, i.Quantidade, i.PrecoUnitario)).ToList();
        await _service.RegistrarDistribuicaoAsync(request.ClienteId, request.ExecucaoId, items, request.DataAporte, request.ValorAporte, request.Parcela, ct);
        return NoContent();
    }

    [HttpPost("{clienteId:long}/custodia/venda")]
    [ProducesResponseType(typeof(VendaCustodiaResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendaCustodiaResultDto>> VendaCustodia(
        [FromRoute] long clienteId,
        [FromBody] VendaCustodiaRequest request,
        CancellationToken ct = default)
    {
        if (request is null || request.Quantidade <= 0) return BadRequest(new { codigo = "QUANTIDADE_INVALIDA", erro = "Quantidade deve ser positiva." });
        var result = await _service.VenderAtivoAsync(clienteId, request.Ticker ?? "", request.Quantidade, request.PrecoVenda, ct);
        if (result is null) return NotFound(new { codigo = "CLIENTE_OU_POSICAO_NAO_ENCONTRADA", erro = "Cliente ou posição não encontrada." });
        return Ok(result);
    }

    [HttpPost("{clienteId:long}/custodia/compra")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompraCustodia(
        [FromRoute] long clienteId,
        [FromBody] CompraCustodiaRequest request,
        CancellationToken ct = default)
    {
        if (request is null || request.Quantidade <= 0) return BadRequest(new { codigo = "QUANTIDADE_INVALIDA", erro = "Quantidade deve ser positiva." });
        var registered = await _service.RegistrarCompraRebalanceamentoAsync(clienteId, request.Ticker ?? "", request.Quantidade, request.PrecoUnitario, ct);
        if (!registered) return NotFound(new { codigo = "CLIENTE_NAO_ENCONTRADO", erro = "Cliente não encontrado." });
        return NoContent();
    }
}

public record VendaCustodiaRequest(string Ticker, int Quantidade, decimal PrecoVenda);
public record CompraCustodiaRequest(string Ticker, int Quantidade, decimal PrecoUnitario);
