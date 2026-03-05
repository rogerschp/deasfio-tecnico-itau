using Admin.Service.Application.DTOs;
using Admin.Service.Application.Ports;
using Admin.Service.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Admin;
namespace Admin.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly ICestaAppService _cestaService;
    private readonly ICustodiaMasterClient _custodiaMasterClient;
    public AdminController(ICestaAppService cestaService, ICustodiaMasterClient custodiaMasterClient)
    {
        _cestaService = cestaService;
        _custodiaMasterClient = custodiaMasterClient;
    }

    [HttpPost("cesta")]
    [ProducesResponseType(typeof(CestaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CestaResponseDto>> CadastrarCesta([FromBody] CestaRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request?.Nome))
            return BadRequest(new { codigo = "NOME_INVALIDO", erro = "Nome da cesta é obrigatório." });
        if (request.Itens == null || request.Itens.Count == 0)
            return BadRequest(new { codigo = "ITENS_VAZIOS", erro = "Informe os 5 ativos da cesta." });
        try
        {
            var items = request.Itens.Select(i => (i.Ticker, i.Percentual)).ToList();
            var result = await _cestaService.CadastrarOuAlterarAsync(request.Nome.Trim(), items, ct);
            return CreatedAtAction(nameof(GetCestaAtual), null, result);
        }
        catch (ArgumentException ex)
        {
            var code = ex.Message.Contains("exatamente 5") ? "QUANTIDADE_ATIVOS_INVALIDA" : "PERCENTUAIS_INVALIDOS";
            return BadRequest(new { codigo = code, erro = ex.Message });
        }
    }

    [HttpGet("cesta/atual")]
    [ProducesResponseType(typeof(CestaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CestaResponseDto>> GetCestaAtual(CancellationToken ct = default)
    {
        var result = await _cestaService.GetAtualAsync(ct);
        if (result is null) return NotFound(new { codigo = "CESTA_NAO_ENCONTRADA", erro = "Nenhuma cesta ativa encontrada." });
        return Ok(result);
    }

    [HttpGet("cesta/vigente")]
    [ProducesResponseType(typeof(CestaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CestaResponseDto>> GetCestaVigente(CancellationToken ct = default)
    {
        var result = await _cestaService.GetVigenteAsync(ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("cesta/historico")]
    [ProducesResponseType(typeof(CestaHistoricoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CestaHistoricoDto>> GetCestaHistorico(CancellationToken ct = default)
    {
        var result = await _cestaService.GetHistoricoAsync(ct);
        return Ok(result);
    }

    [HttpGet("conta-master/custodia")]
    [ProducesResponseType(typeof(CustodiaMasterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CustodiaMasterResponseDto>> GetContaMasterCustodia(CancellationToken ct = default)
    {
        var result = await _custodiaMasterClient.GetCustodiaMasterAsync(ct);
        if (result is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { codigo = "MOTOR_INDISPONIVEL", erro = "Serviço do Motor indisponível." });
        return Ok(result);
    }
}
