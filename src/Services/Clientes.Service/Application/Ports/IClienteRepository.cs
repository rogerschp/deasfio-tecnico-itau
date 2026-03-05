using Clientes.Service.Application.Entities;
namespace Clientes.Service.Application.Ports;
public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Cliente?> GetByCpfAsync(string cpf, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> GetAtivosAsync(CancellationToken ct = default);
    Task<Cliente> SalvarAsync(Cliente cliente, CancellationToken ct = default);
}
public interface IContaGraficaRepository
{
    Task<ContaGrafica?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ContaGrafica> CriarAsync(long clienteId, CancellationToken ct = default);
}
public interface ICustodiaRepository
{
    Task<IReadOnlyList<CustodiaFilhote>> GetPorContaAsync(long contaGraficaId, CancellationToken ct = default);
    Task<CustodiaFilhote?> GetPorContaETickerAsync(long contaGraficaId, string ticker, CancellationToken ct = default);
    Task AdicionarOuAtualizarAsync(long contaGraficaId, string ticker, int quantidade, decimal precoUnitario, CancellationToken ct = default);
    Task<(decimal ValorVenda, decimal Lucro)?> VenderAsync(long contaGraficaId, string ticker, int quantidade, decimal precoVenda, CancellationToken ct = default);
}
