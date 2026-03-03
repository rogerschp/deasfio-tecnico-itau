namespace Cotacao.Domain;

/// <summary>
/// Entidade de cotação histórica B3 (COTAHIST).
/// Representa um registro de pregao por ativo (DataPregao + Ticker únicos).
/// </summary>
public class CotacaoB3
{
    public long Id { get; set; }
    public DateOnly DataPregao { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public decimal PrecoAbertura { get; set; }
    public decimal PrecoFechamento { get; set; }
    public decimal PrecoMaximo { get; set; }
    public decimal PrecoMinimo { get; set; }
}
