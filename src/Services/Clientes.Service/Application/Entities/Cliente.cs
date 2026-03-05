namespace Clientes.Service.Application.Entities;
public class Cliente
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal ValorMensal { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataAdesao { get; set; }
    public DateTime? DataSaida { get; set; }
    public long? ContaGraficaId { get; set; }
}
public class ContaGrafica
{
    public long Id { get; set; }
    public string NumeroConta { get; set; } = string.Empty;
    public string Tipo { get; set; } = "FILHOTE";
    public DateTime DataCriacao { get; set; }
    public long ClienteId { get; set; }
}
public class CustodiaFilhote
{
    public long Id { get; set; }
    public long ContaGraficaId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
}
