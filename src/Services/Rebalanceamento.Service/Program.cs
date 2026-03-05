using Microsoft.EntityFrameworkCore;
using Rebalanceamento.Service.Application.Ports;
using Rebalanceamento.Service.Application.Services;
using Rebalanceamento.Service.Infrastructure;
using Rebalanceamento.Service.Infrastructure.Clients;
using Rebalanceamento.Service.Infrastructure.Persistence;
using Shared.Kafka;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Motor de Rebalanceamento",
        Version = "v1",
        Description = "Rebalanceamento por mudança de cesta (RN-045 a RN-049) e por desvio de proporção (RN-050 a RN-052). IR sobre vendas (RN-057 a RN-062) publicada no Kafka."
    });
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});
builder.Services.Configure<RebalanceamentoServiceOptions>(builder.Configuration.GetSection(RebalanceamentoServiceOptions.SectionName));
builder.Services.AddHttpClient<IClientesRebalanceamentoClient, HttpClientesRebalanceamentoClient>();
builder.Services.AddHttpClient<ICestaVigenteClient, HttpCestaVigenteClient>();
builder.Services.AddHttpClient<ICotacaoFechamentoClient, HttpCotacaoFechamentoClient>();
var conn = builder.Configuration.GetConnectionString("Rebalanceamento") ?? "Server=localhost;Port=3306;Database=rebalanceamento_db;User=root;Password=root;CharSet=utf8mb4;";
builder.Services.AddDbContext<RebalanceamentoDbContext>(options =>
{
    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
});
builder.Services.AddScoped<IVendaRebalanceamentoRepository, VendaRebalanceamentoRepository>();
builder.Services.AddKafkaEventoIR(builder.Configuration);
builder.Services.AddScoped<IExecutarRebalanceamentoService, ExecutarRebalanceamentoService>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rebalanceamento v1");
        c.RoutePrefix = "swagger";
    });
}
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    var requestId = context.Request.Headers["X-Request-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
    context.Response.Headers["X-Request-Id"] = requestId;
    await next();
});
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RebalanceamentoDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();
