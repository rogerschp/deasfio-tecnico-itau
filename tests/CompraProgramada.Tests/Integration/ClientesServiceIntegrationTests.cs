using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Clientes.Service.Application.DTOs;
using Clientes.Service.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Contracts.Clientes;
namespace CompraProgramada.Tests.Integration;
public class ClientesServiceIntegrationTests : IClassFixture<WebApplicationFactory<ClientesController>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _client;
    public ClientesServiceIntegrationTests(WebApplicationFactory<ClientesController> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"))
            .CreateClient();
    }
    [Fact]
    public async Task Adesao_PostComDadosValidos_Retorna201EClienteCriado()
    {
        var request = new AdesaoRequest("Maria Integração", "98765432100", "maria@test.com", 500m);
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var dto = await response.Content.ReadFromJsonAsync<AdesaoResponseDto>(JsonOptions);
        Assert.NotNull(dto);
        Assert.Equal("Maria Integração", dto.Nome);
        Assert.Equal(500m, dto.ValorMensal);
        Assert.True(dto.Ativo);
    }
    [Fact]
    public async Task Adesao_PostComBodyVazio_Retorna400()
    {
        var response = await _client.PostAsJsonAsync<AdesaoRequest>("/api/clientes/adesao", null!);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task GetById_ClienteInexistente_Retorna404()
    {
        var response = await _client.GetAsync("/api/clientes/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task AlterarValorMensal_SemBody_Retorna400()
    {
        var response = await _client.PutAsJsonAsync<object>("/api/clientes/1/valor-mensal", null!);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task Response_IncluiHeaderXRequestId()
    {
        var response = await _client.GetAsync("/api/clientes/99999");
        Assert.True(response.Headers.Contains("X-Request-Id"));
    }
}
