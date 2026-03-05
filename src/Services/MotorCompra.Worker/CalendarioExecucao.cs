namespace MotorCompra.Worker;

public static class CalendarioExecucao
{
    public static readonly IReadOnlyList<int> DiasExecucaoCompra = new[] { 5, 15, 25 };

    public static bool EhDiaDeExecucao(DateOnly date)
    {
        var (year, month) = (date.Year, date.Month);
        foreach (var day in DiasExecucaoCompra)
        {
            var targetDate = new DateOnly(year, month, Math.Min(day, DateTime.DaysInMonth(year, month)));
            var effectiveDate = ObterProximoDiaUtil(targetDate);
            if (date == effectiveDate)
                return true;
        }
        return false;
    }

    private static DateOnly ObterProximoDiaUtil(DateOnly date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Saturday => date.AddDays(2),
            DayOfWeek.Sunday => date.AddDays(1),
            _ => date
        };
    }
}
