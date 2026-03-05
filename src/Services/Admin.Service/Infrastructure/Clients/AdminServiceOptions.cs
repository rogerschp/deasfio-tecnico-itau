namespace Admin.Service.Infrastructure.Clients;
public class AdminServiceOptions
{
    public const string SectionName = "Admin";
    public string CotacaoServiceBaseUrl { get; set; } = "http://localhost:5003";
    public string MotorServiceBaseUrl { get; set; } = "http://localhost:5004";
}
