using MotorCompra.Worker;
using Microsoft.Extensions.Options;
var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection(WorkerOptions.SectionName));
builder.Services.AddHttpClient(Worker.HttpClientName, static (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<WorkerOptions>>().Value;
    client.BaseAddress = new Uri(options.MotorCompraServiceBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHostedService<Worker>();
var host = builder.Build();
host.Run();
