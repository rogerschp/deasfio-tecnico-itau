namespace Shared.Contracts.Clientes;
public record AdesaoRequest(
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensal);
