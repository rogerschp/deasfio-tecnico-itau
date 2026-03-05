using Microsoft.EntityFrameworkCore;
using MotorCompra.Service.Application.Ports;
using MotorCompra.Service.Application.Services;
using MotorCompra.Service.Infrastructure.Clients;
using MotorCompra.Service.Infrastructure.Persistence;
using Shared.Kafka;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Motor de Compra Programada",
        Version = "v1",
        Description = "Execução da compra programada nos dias 5, 15 e 25; agrupamento, cotações, distribuição e IR dedo-duro (Kafka)."
    });
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

builder.Services.Configure<MotorCompraServiceOptions>(builder.Configuration.GetSection(MotorCompraServiceOptions.SectionName));
builder.Services.AddHttpClient<ICestaVigenteClient, HttpCestaVigenteClient>();
builder.Services.AddHttpClient<IClientesAtivosClient, HttpClientesAtivosClient>();
builder.Services.AddHttpClient<ICotacaoFechamentoClient, HttpCotacaoFechamentoClient>();
builder.Services.AddHttpClient<IRegistroDistribuicaoClient, HttpRegistroDistribuicaoClient>();

var conn = builder.Configuration.GetConnectionString("Motor") ?? "Server=localhost;Port=3306;Database=motor_db;User=root;Password=root;CharSet=utf8mb4;";
builder.Services.AddDbContext<MotorDbContext>(options =>
{
    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
});
builder.Services.AddScoped<IExecucaoCompraRepository, ExecucaoCompraRepository>();
builder.Services.AddScoped<ICustodiaMasterRepository, CustodiaMasterRepository>();

builder.Services.AddKafkaEventoIR(builder.Configuration);

builder.Services.AddScoped<IExecutarCompraProgramadaService, ExecutarCompraProgramadaService>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Motor Compra v1");
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
    var db = scope.ServiceProvider.GetRequiredService<MotorDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();
