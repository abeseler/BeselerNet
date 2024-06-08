using Dapper;
using System.Data;

namespace Beseler.ApiService.Application;

internal sealed class SqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly date)
        => parameter.Value = date.ToDateTime(TimeOnly.MinValue);
    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
}
