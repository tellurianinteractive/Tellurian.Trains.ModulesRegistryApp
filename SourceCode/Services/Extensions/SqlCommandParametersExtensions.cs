using Microsoft.Data.SqlClient;

namespace ModulesRegistry.Services.Extensions;
internal static class SqlCommandParametersExtensions
{
    public static SqlParameter AddWithNullableValue<T>(this SqlParameterCollection parameters, string parameterName, T? value) where T : class
    {
        if (value is null)
            return parameters.AddWithValue(parameterName, DBNull.Value);
        else
            return parameters.AddWithValue(parameterName, value);
    }
    public static SqlParameter AddWithNullableValue<T>(this SqlParameterCollection parameters, string parameterName, T? value) where T : struct
    {
        if (value.HasValue)
            return parameters.AddWithValue(parameterName, value.Value);
        else
            return parameters.AddWithValue(parameterName, DBNull.Value);
    }

}
