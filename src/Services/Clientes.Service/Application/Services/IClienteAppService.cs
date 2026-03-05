using Clientes.Service.Application.DTOs;
namespace Clientes.Service.Application.Services;
public interface IClienteAppService
{
    Task<AdesaoResponseDto> AderirAsync(string nome, string cpf, string email, decimal valorMensal, CancellationToken ct = default);
    Task<SaidaResponseDto?> SairAsync(long clienteId, CancellationToken ct = default);
    Task<AlterarValorResponseDto?> AlterarValorMensalAsync(long clienteId, decimal novoValorMensal, CancellationToken ct = default);
    Task<AdesaoResponseDto?> GetByIdAsync(long clienteId, CancellationToken ct = default);
    Task<IReadOnlyList<ClienteAtivoDto>> GetAtivosAsync(CancellationToken ct = default);
    Task<CarteiraResponseDto?> GetCarteiraAsync(long clienteId, CancellationToken ct = default);
    Task RegistrarDistribuicaoAsync(long clienteId, long execucaoId, IReadOnlyList<(string Ticker, int Quantidade, decimal PrecoUnitario)> itens, DateOnly? dataAporte = null, decimal? valorAporte = null, int? parcela = null, CancellationToken ct = default);
    Task<RentabilidadeResponseDto?> GetRentabilidadeAsync(long clienteId, CancellationToken ct = default);
    Task<VendaCustodiaResultDto?> VenderAtivoAsync(long clienteId, string ticker, int quantidade, decimal precoVenda, CancellationToken ct = default);
    Task<bool> RegistrarCompraRebalanceamentoAsync(long clienteId, string ticker, int quantidade, decimal precoUnitario, CancellationToken ct = default);
}
