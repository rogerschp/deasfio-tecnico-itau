using Clientes.Service.Application.DTOs;
using Clientes.Service.Application.Entities;
using Clientes.Service.Application.Ports;
namespace Clientes.Service.Application.Services;
public class ClienteAppService : IClienteAppService
{
    private const decimal ValorMensalMinimo = 100m;
    private static string CpfNormalizado(string cpf) => new string(cpf.Where(char.IsDigit).ToArray());
    private readonly IClienteRepository _clienteRepo;
    private readonly IContaGraficaRepository _contaRepo;
    private readonly ICustodiaRepository _custodiaRepo;
    private readonly ICotacaoFechamentoClient? _cotacaoClient;
    private readonly IAporteRepository _aporteRepo;
    public ClienteAppService(IClienteRepository clienteRepo, IContaGraficaRepository contaRepo, ICustodiaRepository custodiaRepo, IAporteRepository aporteRepo, ICotacaoFechamentoClient? cotacaoClient = null)
    {
        _clienteRepo = clienteRepo;
        _contaRepo = contaRepo;
        _custodiaRepo = custodiaRepo;
        _aporteRepo = aporteRepo;
        _cotacaoClient = cotacaoClient;
    }
    public async Task<AdesaoResponseDto> AderirAsync(string nome, string cpf, string email, decimal valorMensal, CancellationToken ct = default)
    {
        if (valorMensal < ValorMensalMinimo)
            throw new ArgumentException($"O valor mensal mínimo é de R$ {ValorMensalMinimo:N2}.", nameof(valorMensal));
        var cpfNorm = CpfNormalizado(cpf);
        if (string.IsNullOrEmpty(cpfNorm) || cpfNorm.Length != 11)
            throw new ArgumentException("CPF inválido.", nameof(cpf));
        var existente = await _clienteRepo.GetByCpfAsync(cpfNorm, ct);
        if (existente != null)
            throw new InvalidOperationException("CPF já cadastrado no sistema.");
        var cliente = new Cliente
        {
            Nome = nome.Trim(),
            Cpf = cpfNorm,
            Email = email.Trim(),
            ValorMensal = valorMensal,
            Ativo = true,
            DataAdesao = DateTime.UtcNow
        };
        cliente = await _clienteRepo.SalvarAsync(cliente, ct);
        var conta = await _contaRepo.CriarAsync(cliente.Id, ct);
        cliente.ContaGraficaId = conta.Id;
        await _clienteRepo.SalvarAsync(cliente, ct);
        return new AdesaoResponseDto(
            cliente.Id, cliente.Nome, cliente.Cpf, cliente.Email, cliente.ValorMensal, true, cliente.DataAdesao,
            new ContaGraficaDto(conta.Id, conta.NumeroConta, conta.Tipo, conta.DataCriacao));
    }
    public async Task<SaidaResponseDto?> SairAsync(long clienteId, CancellationToken ct = default)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null) return null;
        if (!cliente.Ativo)
            throw new InvalidOperationException("Cliente já havia saído do produto.");
        cliente.Ativo = false;
        cliente.DataSaida = DateTime.UtcNow;
        await _clienteRepo.SalvarAsync(cliente, ct);
        return new SaidaResponseDto(cliente.Id, cliente.Nome, false, cliente.DataSaida.Value, "Adesão encerrada. Sua posição em custódia foi mantida.");
    }
    public async Task<AlterarValorResponseDto?> AlterarValorMensalAsync(long clienteId, decimal novoValorMensal, CancellationToken ct = default)
    {
        if (novoValorMensal < ValorMensalMinimo)
            throw new ArgumentException($"O valor mensal mínimo é de R$ {ValorMensalMinimo:N2}.", nameof(novoValorMensal));
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null) return null;
        var anterior = cliente.ValorMensal;
        cliente.ValorMensal = novoValorMensal;
        await _clienteRepo.SalvarAsync(cliente, ct);
        return new AlterarValorResponseDto(cliente.Id, anterior, novoValorMensal, DateTime.UtcNow, "Valor mensal atualizado. O novo valor será considerado a partir da próxima data de compra.");
    }
    public async Task<AdesaoResponseDto?> GetByIdAsync(long clienteId, CancellationToken ct = default)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null) return null;
        ContaGraficaDto? contaDto = null;
        if (cliente.ContaGraficaId.HasValue)
        {
            var conta = await _contaRepo.GetByIdAsync(cliente.ContaGraficaId.Value, ct);
            if (conta != null)
                contaDto = new ContaGraficaDto(conta.Id, conta.NumeroConta, conta.Tipo, conta.DataCriacao);
        }
        return new AdesaoResponseDto(cliente.Id, cliente.Nome, cliente.Cpf, cliente.Email, cliente.ValorMensal, cliente.Ativo, cliente.DataAdesao, contaDto);
    }
    public async Task<IReadOnlyList<ClienteAtivoDto>> GetAtivosAsync(CancellationToken ct = default)
    {
        var clientes = await _clienteRepo.GetAtivosAsync(ct);
        var result = new List<ClienteAtivoDto>();
        foreach (var c in clientes)
        {
            if (!c.ContaGraficaId.HasValue) continue;
            var conta = await _contaRepo.GetByIdAsync(c.ContaGraficaId.Value, ct);
            if (conta is null) continue;
            result.Add(new ClienteAtivoDto(c.Id, c.Nome, c.Cpf, c.ValorMensal, conta.Id));
        }
        return result;
    }
    public async Task<CarteiraResponseDto?> GetCarteiraAsync(long clienteId, CancellationToken ct = default)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null || !cliente.ContaGraficaId.HasValue) return null;
        var conta = await _contaRepo.GetByIdAsync(cliente.ContaGraficaId.Value, ct);
        if (conta is null) return null;
        var custodia = await _custodiaRepo.GetPorContaAsync(conta.Id, ct);
        var tickerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < custodia.Count; i++)
            tickerSet.Add(custodia[i].Ticker);
        var tickers = new List<string>(tickerSet);
        var precoPorTicker = new Dictionary<string, decimal>(tickers.Count, StringComparer.OrdinalIgnoreCase);
        if (tickers.Count > 0 && _cotacaoClient != null)
        {
            var cotacoes = await _cotacaoClient.GetFechamentosAsync(tickers, ct);
            foreach (var q in cotacoes)
                precoPorTicker[q.Ticker] = q.PrecoFechamento;
        }
        decimal valorInvestido = 0, valorAtual = 0;
        var ativos = new List<AtivoCarteiraDto>();
        foreach (var c in custodia)
        {
            valorInvestido += c.Quantidade * c.PrecoMedio;
            var cotacaoAtual = precoPorTicker.GetValueOrDefault(c.Ticker, c.PrecoMedio);
            valorAtual += c.Quantidade * cotacaoAtual;
            var pl = (cotacaoAtual - c.PrecoMedio) * c.Quantidade;
            var plPct = c.PrecoMedio > 0 ? (cotacaoAtual - c.PrecoMedio) / c.PrecoMedio * 100 : 0;
            ativos.Add(new AtivoCarteiraDto(c.Ticker, c.Quantidade, c.PrecoMedio, cotacaoAtual, c.Quantidade * cotacaoAtual, pl, (decimal)plPct, 0));
        }
        if (valorAtual > 0)
            for (var i = 0; i < ativos.Count; i++)
            {
                var a = ativos[i];
                ativos[i] = a with { ComposicaoCarteira = a.ValorAtual / valorAtual * 100 };
            }
        var plTotal = valorAtual - valorInvestido;
        var rentPct = valorInvestido > 0 ? (valorAtual - valorInvestido) / valorInvestido * 100 : 0;
        return new CarteiraResponseDto(
            cliente.Id, cliente.Nome, conta.NumeroConta, DateTime.UtcNow,
            new CarteiraResumoDto(valorInvestido, valorAtual, plTotal, (decimal)rentPct),
            ativos);
    }
    public async Task RegistrarDistribuicaoAsync(long clienteId, long execucaoId, IReadOnlyList<(string Ticker, int Quantidade, decimal PrecoUnitario)> itens, DateOnly? dataAporte = null, decimal? valorAporte = null, int? parcela = null, CancellationToken ct = default)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null || !cliente.ContaGraficaId.HasValue) return;
        foreach (var (ticker, qtd, preco) in itens)
            if (qtd > 0)
                await _custodiaRepo.AdicionarOuAtualizarAsync(cliente.ContaGraficaId.Value, ticker, qtd, preco, ct);
        if (dataAporte.HasValue && valorAporte.HasValue && parcela.HasValue && parcela.Value >= 1 && parcela.Value <= 3)
            await _aporteRepo.RegistrarAsync(clienteId, dataAporte.Value, valorAporte.Value, parcela.Value, ct);
    }
    public async Task<RentabilidadeResponseDto?> GetRentabilidadeAsync(long clienteId, CancellationToken ct = default)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null) return null;
        var carteira = await GetCarteiraAsync(clienteId, ct);
        if (carteira is null) return null;
        var aportes = await _aporteRepo.GetPorClienteOrdenadoPorDataAsync(clienteId, ct);
        var historicoAportes = new List<HistoricoAporteDto>(aportes.Count);
        decimal totalAportado = 0;
        for (var i = 0; i < aportes.Count; i++)
        {
            var a = aportes[i];
            totalAportado += a.Valor;
            historicoAportes.Add(new HistoricoAporteDto(
                a.DataAporte.ToString("yyyy-MM-dd"),
                a.Valor,
                $"{a.Parcela}/3"));
        }
        decimal valorInvestidoAcum = 0;
        var evolucaoCarteira = new List<EvolucaoCarteiraDto>();
        for (var i = 0; i < aportes.Count; i++)
        {
            valorInvestidoAcum += aportes[i].Valor;
            var isUltimo = i == aportes.Count - 1;
            var valorCarteira = isUltimo ? carteira.Resumo.ValorAtualCarteira : valorInvestidoAcum;
            var rentPct = valorInvestidoAcum > 0 ? (valorCarteira - valorInvestidoAcum) / valorInvestidoAcum * 100 : 0;
            evolucaoCarteira.Add(new EvolucaoCarteiraDto(
                aportes[i].DataAporte.ToString("yyyy-MM-dd"),
                valorCarteira,
                valorInvestidoAcum,
                Math.Round(rentPct, 2)));
        }
        var valorAtual = carteira.Resumo.ValorAtualCarteira;
        var plTotal = valorAtual - totalAportado;
        var rentabilidadePct = totalAportado > 0 ? (valorAtual - totalAportado) / totalAportado * 100 : 0;
        var resumoRentabilidade = new CarteiraResumoDto(totalAportado, valorAtual, plTotal, Math.Round((decimal)rentabilidadePct, 2));
        return new RentabilidadeResponseDto(
            clienteId,
            cliente.Nome,
            DateTime.UtcNow,
            resumoRentabilidade,
            historicoAportes,
            evolucaoCarteira);
    }
    public async Task<VendaCustodiaResultDto?> VenderAtivoAsync(long clienteId, string ticker, int quantidade, decimal precoVenda, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ticker) || quantidade <= 0) return null;
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null || !cliente.ContaGraficaId.HasValue) return null;
        var result = await _custodiaRepo.VenderAsync(cliente.ContaGraficaId.Value, ticker.Trim().ToUpperInvariant(), quantidade, precoVenda, ct);
        return result is null ? null : new VendaCustodiaResultDto(result.Value.ValorVenda, result.Value.Lucro);
    }
    public async Task<bool> RegistrarCompraRebalanceamentoAsync(long clienteId, string ticker, int quantidade, decimal precoUnitario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ticker) || quantidade <= 0) return false;
        var cliente = await _clienteRepo.GetByIdAsync(clienteId, ct);
        if (cliente is null || !cliente.ContaGraficaId.HasValue) return false;
        await _custodiaRepo.AdicionarOuAtualizarAsync(cliente.ContaGraficaId.Value, ticker.Trim().ToUpperInvariant(), quantidade, precoUnitario, ct);
        return true;
    }
}
