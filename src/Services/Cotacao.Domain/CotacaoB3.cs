namespace Cotacao.Domain;

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
