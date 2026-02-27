namespace Shared.Contracts.Eventos;

/// <summary>
/// Mensagem publicada no tópico Kafka para IR sobre vendas (20% quando vendas no mês > R$ 20.000).
/// </summary>
public record EventoIRVenda(
    string Tipo,
    long ClienteId,
    string Cpf,
    string MesReferencia,
    decimal TotalVendasMes,
    decimal LucroLiquido,
    decimal Aliquota,
    decimal ValorIR,
    IReadOnlyList<DetalheVenda> Detalhes,
    DateTime DataCalculo);

public record DetalheVenda(
    string Ticker,
    int Quantidade,
    decimal PrecoVenda,
    decimal PrecoMedio,
    decimal Lucro);
