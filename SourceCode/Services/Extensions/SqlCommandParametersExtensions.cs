using Microsoft.Data.SqlClient;

namespace ModulesRegistry.Services.Extensions;
internal static class SqlCommandParametersExtensions
{
    public static SqlParameter AddWithNullableValue<T>(this SqlParameterCollection collection, string parameterName, T? value) where T : class
    {
        if (value is null)
            return collection.AddWithValue(parameterName, DBNull.Value);
        else
            return collection.AddWithValue(parameterName, value);
    }
    public static SqlParameter AddWithNullableValue<T>(this SqlParameterCollection collection, string parameterName, T? value) where T : struct
    {
        if (value.HasValue)
            return collection.AddWithValue(parameterName, value.Value);
        else
            return collection.AddWithValue(parameterName, DBNull.Value);
    }

}
