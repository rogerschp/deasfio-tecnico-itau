namespace Shared.Contracts.Clientes;
public record ClienteResponse(
    long ClienteId,
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensal,
    bool Ativo,
    DateTime DataAdesao,
    ContaGraficaResponse? ContaGrafica);
public record ContaGraficaResponse(
    long Id,
    string NumeroConta,
    string Tipo,
    DateTime DataCriacao);
