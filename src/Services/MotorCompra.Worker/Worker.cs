using System.Net.Http.Json;
using Microsoft.Extensions.Options;
namespace MotorCompra.Worker;
public class Worker : BackgroundService
{
    public const string HttpClientName = "MotorCompra";
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpFactory;
    private readonly WorkerOptions _options;
    public Worker(ILogger<Worker> logger, IHttpClientFactory httpFactory, IOptions<WorkerOptions> options)
    {
        _logger = logger;
        _httpFactory = httpFactory;
        _options = options.Value;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Motor de Compra Worker iniciado. Verificando datas de execução (dias 5, 15 e 25).");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
                if (CalendarioExecucao.EhDiaDeExecucao(hoje))
                {
                    _logger.LogInformation("Data de execução detectada: {Data}. Disparando compra programada.", hoje);
                    var client = _httpFactory.CreateClient(HttpClientName);
                    var response = await client.PostAsJsonAsync(
                        "api/motor/executar-compra",
                        new { DataReferencia = hoje },
                        stoppingToken);
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                            _logger.LogInformation("Execução já realizada para {Data} ou sem dados (cesta/clientes/cotação).", hoje);
                        else
                            _logger.LogInformation("Compra programada executada com sucesso para {Data}.", hoje);
                    }
                    else
                        _logger.LogWarning("Falha ao executar compra: {StatusCode} - {Reason}", response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar ou executar compra programada.");
            }
            await Task.Delay(TimeSpan.FromMinutes(_options.IntervaloMinutos), stoppingToken);
        }
    }
}
public class WorkerOptions
{
    public const string SectionName = "MotorCompraWorker";
    public string MotorCompraServiceBaseUrl { get; set; } = "http://localhost:5004";
    public int IntervaloMinutos { get; set; } = 60;
}
