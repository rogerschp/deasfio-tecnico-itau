namespace Admin.Service.Application.DTOs;
public record CestaResponseDto(
    long CestaId,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    IReadOnlyList<ItemCestaDto> Itens,
    bool RebalanceamentoDisparado = false,
    CestaDesativadaDto? CestaAnteriorDesativada = null,
    IReadOnlyList<string>? AtivosRemovidos = null,
    IReadOnlyList<string>? AtivosAdicionados = null,
    string? Mensagem = null);
public record ItemCestaDto(string Ticker, decimal Percentual, decimal? CotacaoAtual = null);
public record CestaDesativadaDto(long CestaId, string Nome, DateTime DataDesativacao);
public record CestaHistoricoDto(IReadOnlyList<CestaHistoricoItemDto> Cestas);
public record CestaHistoricoItemDto(
    long CestaId,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    DateTime? DataDesativacao,
    IReadOnlyList<ItemCestaDto> Itens);
