namespace Shared.Contracts.Admin;
public record CestaRequest(
    string Nome,
    IReadOnlyList<ItemCestaRequest> Itens);
public record ItemCestaRequest(
    string Ticker,
    decimal Percentual);
