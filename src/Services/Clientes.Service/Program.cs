using Clientes.Service.Application.Services;
using Clientes.Service.Application.Ports;
using Clientes.Service.Infrastructure.Persistence;
using Clientes.Service.Infrastructure.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clientes Service - Compra Programada",
        Version = "v1",
        Description = "API de clientes: adesão, saída, valor mensal, carteira, rentabilidade e distribuição (Motor)."
    });
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});
builder.Services.Configure<ClientesServiceOptions>(builder.Configuration.GetSection(ClientesServiceOptions.SectionName));
builder.Services.AddHttpClient<ICotacaoFechamentoClient, HttpCotacaoFechamentoClient>();
var useInMemory = string.Equals(builder.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase);
if (useInMemory)
    builder.Services.AddDbContext<ClientesDbContext>(options => options.UseInMemoryDatabase("ClientesTestDb"));
else
{
    var conn = builder.Configuration.GetConnectionString("Clientes") ?? "Server=localhost;Port=3306;Database=clientes_db;User=root;Password=root;CharSet=utf8mb4;";
    builder.Services.AddDbContext<ClientesDbContext>(options => options.UseMySql(conn, ServerVersion.AutoDetect(conn)));
}
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IContaGraficaRepository, ContaGraficaRepository>();
builder.Services.AddScoped<ICustodiaRepository, CustodiaRepository>();
builder.Services.AddScoped<IAporteRepository, AporteRepository>();
builder.Services.AddScoped<IClienteAppService, ClienteAppService>();
var app = builder.Build();
var swaggerEnabled = app.Environment.IsDevelopment() ||
    string.Equals(builder.Configuration["Swagger:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clientes Service v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Clientes Service - Compra Programada";
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
if (!useInMemory)
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ClientesDbContext>();
        await db.Database.MigrateAsync();
    }
}
app.Run();
