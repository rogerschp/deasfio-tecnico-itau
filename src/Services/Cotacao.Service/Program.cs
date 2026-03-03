using Cotacao.Application.Services;
using Cotacao.Infrastructure;
using Cotacao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cotação Service (COTAHIST B3)",
        Version = "v1",
        Description = "API de cotações históricas B3: consulta de fechamento por ticker e importação de arquivo COTAHIST."
    });
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddCotacaoInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICotacaoAppService, CotacaoAppService>();

var app = builder.Build();

// Swagger: habilitado em Development; em outros ambientes use "Swagger:Enabled" = true em config
var swaggerEnabled = app.Environment.IsDevelopment() ||
    string.Equals(builder.Configuration["Swagger:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cotação Service v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Cotação Service - COTAHIST B3";
    });
}

app.UseHttpsRedirection();
app.MapControllers();

// Aplicar migrations na inicialização (opcional; em produção use job ou script separado)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CotacaoDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
