namespace MotorCompra.Worker;

/// <summary>
/// Define se uma data é dia de execução do motor (dias 5, 15 e 25 ou próximo dia útil - RN-020 a RN-022).
/// </summary>
public static class CalendarioExecucao
{
    private static readonly int[] DiasExecucao = { 5, 15, 25 };

    /// <summary>
    /// Retorna true se a data informada é um dia de execução da compra programada.
    /// </summary>
    public static bool EhDiaDeExecucao(DateOnly data)
    {
        var (ano, mes) = (data.Year, data.Month);
        foreach (var dia in DiasExecucao)
        {
            var dataAlvo = new DateOnly(ano, mes, Math.Min(dia, DateTime.DaysInMonth(ano, mes)));
            var dataEfetiva = ObterProximoDiaUtil(dataAlvo);
            if (data == dataEfetiva)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Se a data for sábado ou domingo, retorna a próxima segunda-feira; caso contrário, retorna a própria data.
    /// </summary>
    private static DateOnly ObterProximoDiaUtil(DateOnly data)
    {
        return data.DayOfWeek switch
        {
            DayOfWeek.Saturday => data.AddDays(2),
            DayOfWeek.Sunday => data.AddDays(1),
            _ => data
        };
    }
}
