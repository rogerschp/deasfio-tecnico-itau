using Shared.Contracts.Clientes;
using Shared.Contracts.Admin;

namespace CompraProgramada.Tests;

public class SharedContractsTests
{
    [Fact]
    public void AdesaoRequest_Deve_Ser_Criado_Com_Valores_Informados()
    {
        var request = new AdesaoRequest("João", "12345678901", "joao@email.com", 1000m);
        Assert.Equal("João", request.Nome);
        Assert.Equal("12345678901", request.Cpf);
        Assert.Equal(1000m, request.ValorMensal);
    }

    [Fact]
    public void CestaRequest_Deve_Conter_Cinco_Itens_Para_Top_Five()
    {
        var itens = new List<ItemCestaRequest>
        {
            new("PETR4", 30m),
            new("VALE3", 25m),
            new("ITUB4", 20m),
            new("BBDC4", 15m),
            new("WEGE3", 10m)
        };
        var request = new CestaRequest("Top Five", itens);
        Assert.Equal(5, request.Itens.Count);
        Assert.Equal(100m, request.Itens.Sum(i => i.Percentual));
    }
}
