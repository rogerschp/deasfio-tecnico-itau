using Cotacao.Application.DTOs;
using Cotacao.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cotacao.Service.Controllers;

/// <summary>
/// API de cotações B3 (COTAHIST): fechamento do último pregão e importação de arquivo.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CotacaoController : ControllerBase
{
    private readonly ICotacaoAppService _cotacaoService;

    public CotacaoController(ICotacaoAppService cotacaoService)
    {
        _cotacaoService = cotacaoService;
    }

    /// <summary>
    /// Obtém a cotação de fechamento do último pregão para um ticker.
    /// </summary>
    /// <param name="ticker">Símbolo do ativo (ex: PETR4, VALE3).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Cotação encontrada.</response>
    /// <response code="404">Ticker não encontrado ou sem dados de pregão.</response>
    [HttpGet("fechamento/{ticker}")]
    [ProducesResponseType(typeof(CotacaoFechamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CotacaoFechamentoDto>> GetFechamento([FromRoute] string ticker, CancellationToken cancellationToken)
    {
        var result = await _cotacaoService.GetFechamentoAsync(ticker, cancellationToken);
        if (result is null)
            return NotFound(new { codigo = "COTACAO_NAO_ENCONTRADA", erro = "Cotação de fechamento não encontrada para o ticker informado." });
        return Ok(result);
    }

    /// <summary>
    /// Obtém cotações de fechamento do último pregão para vários tickers (ex.: para o motor de compra).
    /// </summary>
    /// <param name="tickers">Lista de tickers separados por vírgula (ex: PETR4,VALE3,ITUB4).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpGet("fechamento")]
    [ProducesResponseType(typeof(IReadOnlyList<CotacaoFechamentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CotacaoFechamentoDto>>> GetFechamentos(
        [FromQuery] string? tickers,
        CancellationToken cancellationToken)
    {
        var list = string.IsNullOrWhiteSpace(tickers)
            ? Array.Empty<string>()
            : tickers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = await _cotacaoService.GetFechamentosAsync(list, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Importa um arquivo COTAHIST da B3. O arquivo deve estar acessível no caminho informado (ex: pasta cotacoes/).
    /// Não importa novamente se já existir pregão para a data do arquivo.
    /// </summary>
    /// <param name="request">Caminho do arquivo TXT.</param>
    [HttpPost("importar")]
    [ProducesResponseType(typeof(ImportacaoResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    /// <param name="cancellationToken">Token de cancelamento.</param>
    public async Task<ActionResult<ImportacaoResultDto>> Importar(
        [FromBody] ImportarCotahistRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.CaminhoArquivo))
            return BadRequest(new { codigo = "CAMINHO_INVALIDO", erro = "Caminho do arquivo é obrigatório." });

        try
        {
            var result = await _cotacaoService.ImportarArquivoAsync(request.CaminhoArquivo.Trim(), cancellationToken);
            return Ok(result);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { codigo = "COTACAO_NAO_ENCONTRADA", erro = ex.Message });
        }
    }
}

/// <summary>
/// Request para importação de arquivo COTAHIST.
/// </summary>
public record ImportarCotahistRequest(string CaminhoArquivo);
