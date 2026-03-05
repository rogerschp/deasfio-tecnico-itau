using Clientes.Service.Application.DTOs;
using Clientes.Service.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Clientes;
namespace Clientes.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteAppService _service;
    public ClientesController(IClienteAppService service) => _service = service;

    [HttpPost("adesao")]
    [ProducesResponseType(typeof(AdesaoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdesaoResponseDto>> Adesao([FromBody] AdesaoRequest request, CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Nome) || string.IsNullOrWhiteSpace(request.Cpf))
            return BadRequest(new { codigo = "DADOS_INVALIDOS", erro = "Nome e CPF são obrigatórios." });
        try
        {
            var result = await _service.AderirAsync(request.Nome, request.Cpf, request.Email ?? "", request.ValorMensal, ct);
            return CreatedAtAction(nameof(GetById), new { clienteId = result.ClienteId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { codigo = "VALOR_MENSAL_INVALIDO", erro = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("CPF"))
        {
            return BadRequest(new { codigo = "CLIENTE_CPF_DUPLICADO", erro = ex.Message });
        }
    }

    [HttpPost("{clienteId:long}/saida")]
    [ProducesResponseType(typeof(SaidaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaidaResponseDto>> Saida([FromRoute] long clienteId, CancellationToken ct = default)
    {
        var result = await _service.SairAsync(clienteId, ct);
        if (result is null) return NotFound(new { codigo = "CLIENTE_NAO_ENCONTRADO", erro = "Cliente não encontrado." });
        return Ok(result);
    }

    [HttpPut("{clienteId:long}/valor-mensal")]
    [ProducesResponseType(typeof(AlterarValorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlterarValorResponseDto>> AlterarValorMensal(
        [FromRoute] long clienteId,
        [FromBody] AlterarValorRequest? request,
        CancellationToken ct = default)
    {
        if (request is null)
            return BadRequest(new { codigo = "DADOS_INVALIDOS", erro = "Corpo da requisição é obrigatório." });
        try
        {
            var result = await _service.AlterarValorMensalAsync(clienteId, request.NovoValorMensal, ct);
            if (result is null) return NotFound(new { codigo = "CLIENTE_NAO_ENCONTRADO", erro = "Cliente não encontrado." });
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { codigo = "VALOR_MENSAL_INVALIDO", erro = ex.Message });
        }
    }

    [HttpGet("{clienteId:long}")]
    [ProducesResponseType(typeof(AdesaoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdesaoResponseDto>> GetById([FromRoute] long clienteId, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(clienteId, ct);
        if (result is null) return NotFound(new { codigo = "CLIENTE_NAO_ENCONTRADO", erro = "Cliente não encontrado." });
        return Ok(result);
    }

    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IReadOnlyList<ClienteAtivoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClienteAtivoDto>>> GetAtivos(CancellationToken ct = default)
    {
        var result = await _service.GetAtivosAsync(ct);
        return Ok(result);
    }

    [HttpGet("{clienteId:long}/carteira")]
    [ProducesResponseType(typeof(CarteiraResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CarteiraResponseDto>> GetCarteira([FromRoute] long clienteId, CancellationToken ct = default)
    {
        var result = await _service.GetCarteiraAsync(clienteId, ct);
        if (result is null) return NotFound(new { codigo = "CLIENTE_NAO_ENCONTRADO", erro = "Cliente não encontrado." });
        return Ok(result);
    }

    [HttpGet("{clienteId:long}/rentabilidade")]
    [ProducesResponseType(typeof(RentabilidadeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentabilidadeResponseDto>> GetRentabilidade([FromRoute] long clienteId, CancellationToken ct = default)
    {
        var profitability = await _service.GetRentabilidadeAsync(clienteId, ct);
        if (profitability is null) return NotFound(new { codigo = "CLIENTE_NAO_ENCONTRADO", erro = "Cliente não encontrado." });
        return Ok(profitability);
    }

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
public record AlterarValorRequest(decimal NovoValorMensal);
public record VendaCustodiaRequest(string Ticker, int Quantidade, decimal PrecoVenda);
public record CompraCustodiaRequest(string Ticker, int Quantidade, decimal PrecoUnitario);
