namespace Shared.Contracts.Eventos;

/// <summary>
/// Mensagem publicada no tópico Kafka para IR dedo-duro (0,005% sobre operação).
/// </summary>
public record EventoIRDedoDuro(
    string Tipo,
    long ClienteId,
    string Cpf,
    string Ticker,
    string TipoOperacao,
    int Quantidade,
    decimal PrecoUnitario,
    decimal ValorOperacao,
    decimal Aliquota,
    decimal ValorIR,
    DateTime DataOperacao);
