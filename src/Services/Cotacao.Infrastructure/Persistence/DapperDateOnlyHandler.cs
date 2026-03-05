using System.Data;
using Dapper;
namespace Cotacao.Infrastructure.Persistence;

internal sealed class DapperDateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = new DateTime(value.Year, value.Month, value.Day);
        parameter.DbType = DbType.Date;
    }
    public override DateOnly Parse(object value)
    {
        if (value is DateOnly d)
            return d;
        if (value is DateTime dt)
            return DateOnly.FromDateTime(dt);
        if (value is string s && DateTime.TryParse(s, out var parsed))
            return DateOnly.FromDateTime(parsed);
        throw new InvalidCastException($"Não foi possível converter {value?.GetType().Name ?? "null"} para DateOnly.");
    }
}
