using Admin.Service.Application.Ports;
using Admin.Service.Application.Services;
using Admin.Service.Infrastructure.Clients;
using Admin.Service.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Admin Service - Cesta Top Five",
        Version = "v1",
        Description = "API administrativa: cadastro e consulta da cesta de recomendação Top Five (5 ativos, soma 100%)."
    });
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});
builder.Services.Configure<AdminServiceOptions>(builder.Configuration.GetSection(AdminServiceOptions.SectionName));
builder.Services.AddHttpClient<ICotacaoFechamentoClient, HttpCotacaoFechamentoClient>();
builder.Services.AddHttpClient<ICustodiaMasterClient, HttpCustodiaMasterClient>();
var conn = builder.Configuration.GetConnectionString("Admin") ?? "Server=localhost;Port=3306;Database=admin_db;User=root;Password=root;CharSet=utf8mb4;";
builder.Services.AddDbContext<AdminDbContext>(options => options.UseMySql(conn, ServerVersion.AutoDetect(conn)));
builder.Services.AddScoped<ICestaRepository, CestaRepository>();
builder.Services.AddScoped<ICestaAppService, CestaAppService>();
var app = builder.Build();
var swaggerEnabled = app.Environment.IsDevelopment() ||
    string.Equals(builder.Configuration["Swagger:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin Service v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Admin Service - Cesta Top Five";
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
    var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();
