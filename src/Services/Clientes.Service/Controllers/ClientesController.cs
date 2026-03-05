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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaidaResponseDto>> Saida([FromRoute] long clienteId, CancellationToken ct = default)
    {
        try
        {
            var result = await _service.SairAsync(clienteId, ct);
            if (result is null) return NotFound(new { codigo = "CLIENTE_NAO_ENCONTRADO", erro = "Cliente não encontrado." });
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("saído", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { codigo = "CLIENTE_JA_INATIVO", erro = ex.Message });
        }
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

}
public record AlterarValorRequest(decimal NovoValorMensal);
