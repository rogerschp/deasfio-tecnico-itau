namespace Clientes.Service.Application.DTOs;
public record AdesaoResponseDto(
    long ClienteId,
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensal,
    bool Ativo,
    DateTime DataAdesao,
    ContaGraficaDto? ContaGrafica);
public record ContaGraficaDto(long Id, string NumeroConta, string Tipo, DateTime DataCriacao);
public record SaidaResponseDto(long ClienteId, string Nome, bool Ativo, DateTime DataSaida, string Mensagem);
public record AlterarValorResponseDto(long ClienteId, decimal ValorMensalAnterior, decimal ValorMensalNovo, DateTime DataAlteracao, string Mensagem);

public record ClienteAtivoDto(long ClienteId, string Nome, string Cpf, decimal ValorMensal, long ContaGraficaId);
public record CarteiraResponseDto(
    long ClienteId,
    string Nome,
    string ContaGrafica,
    DateTime DataConsulta,
    CarteiraResumoDto Resumo,
    IReadOnlyList<AtivoCarteiraDto> Ativos);
public record CarteiraResumoDto(decimal ValorTotalInvestido, decimal ValorAtualCarteira, decimal PlTotal, decimal RentabilidadePercentual);
public record AtivoCarteiraDto(string Ticker, int Quantidade, decimal PrecoMedio, decimal CotacaoAtual, decimal ValorAtual, decimal Pl, decimal PlPercentual, decimal ComposicaoCarteira);
public record DistribuicaoRequestDto(long ExecucaoId, long ClienteId, IReadOnlyList<ItemDistribuicaoDto> Itens, DateOnly? DataAporte = null, decimal? ValorAporte = null, int? Parcela = null);
public record ItemDistribuicaoDto(string Ticker, int Quantidade, decimal PrecoUnitario);

public record VendaCustodiaResultDto(decimal ValorVenda, decimal Lucro);

public record RentabilidadeResponseDto(
    long ClienteId,
    string Nome,
    DateTime DataConsulta,
    CarteiraResumoDto Rentabilidade,
    IReadOnlyList<HistoricoAporteDto> HistoricoAportes,
    IReadOnlyList<EvolucaoCarteiraDto> EvolucaoCarteira);
public record HistoricoAporteDto(string Data, decimal Valor, string Parcela);
public record EvolucaoCarteiraDto(string Data, decimal ValorCarteira, decimal ValorInvestido, decimal Rentabilidade);
