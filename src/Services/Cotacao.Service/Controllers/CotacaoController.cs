using Cotacao.Application.DTOs;
using Cotacao.Application.Services;
using Cotacao.Service.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
namespace Cotacao.Service.Controllers;

[ApiController]
[Route("api/cotacoes")]
[Produces("application/json")]
public class CotacaoController : ControllerBase
{
    private readonly ICotacaoAppService _cotacaoService;
    private readonly CotacaoServiceOptions _options;
    public CotacaoController(ICotacaoAppService cotacaoService, IOptions<CotacaoServiceOptions> options)
    {
        _cotacaoService = cotacaoService;
        _options = options.Value;
    }

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

    [HttpPost("importar")]
    [ProducesResponseType(typeof(ImportacaoResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportacaoResultDto>> Importar(
        [FromBody] ImportarCotahistRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.CaminhoArquivo))
            return BadRequest(new { codigo = "CAMINHO_INVALIDO", erro = "Caminho do arquivo é obrigatório." });
        var filePath = request.CaminhoArquivo.Trim();
        if (!Path.IsPathRooted(filePath))
        {
            var basePath = Path.IsPathRooted(_options.PastaCotacoes)
                ? _options.PastaCotacoes
                : ResolverBasePathCotacoes(_options.PastaCotacoes);
            filePath = Path.GetFullPath(Path.Combine(basePath, filePath));
            var baseNorm = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!filePath.StartsWith(baseNorm + Path.DirectorySeparatorChar) && !filePath.Equals(baseNorm))
                return BadRequest(new { codigo = "CAMINHO_INVALIDO", erro = "Caminho do arquivo inválido (fora da pasta de cotações)." });
        }
        try
        {
            var result = await _cotacaoService.ImportarArquivoAsync(filePath, cancellationToken);
            return Ok(result);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { codigo = "COTACAO_NAO_ENCONTRADA", erro = ex.Message });
        }
    }

    private static string ResolverBasePathCotacoes(string cotacoesFolder)
    {
        var currentPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), cotacoesFolder));
        if (Directory.Exists(currentPath))
            return currentPath;
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 10 && !string.IsNullOrEmpty(dir); i++)
        {
            var candidate = Path.GetFullPath(Path.Combine(dir, cotacoesFolder));
            if (Directory.Exists(candidate))
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), cotacoesFolder));
    }
}

public record ImportarCotahistRequest(string CaminhoArquivo);
