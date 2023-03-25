using System.Data;

namespace ModulesRegistry.Services.Extensions;

public static class IDataRecordExtensions
{
    private const bool ThrowOnMissingColumnName = true;
    public static string GetString(this IDataRecord record, string columnName, string defaultValue = "")
    {
        var i = record.GetColumIndex(columnName, false);
        if (i < 0 || record.IsDBNull(i)) return defaultValue;
        var s = record.GetString(record.GetOrdinal(columnName));
        return (string.IsNullOrWhiteSpace(s)) ? defaultValue : s;
    }

    public static byte GetByte(this IDataRecord record, string columnName)
    {
        var i = record.GetColumIndex(columnName);
        if (record.IsDBNull(i)) return 0;
        var value = record.GetValue(i);
        if (value is byte a) return a;
        if (value is int c) return (byte)c;
        if (value is double d) return (byte)d;
        throw new InvalidOperationException(columnName);
    }

    public static int GetInt(this IDataRecord record, string columnName, short defaultValue = 0)
    {
        var i = record.GetColumIndex(columnName, false);
        if (i < 0) return defaultValue;
        if (record.IsDBNull(i)) return defaultValue;
        var value = record.GetValue(i);
        if (value is int b) return b;
        if (value is short a) return a;
        throw new InvalidOperationException(columnName);
    }

    public static int? GetNullableInt(this IDataRecord record, string columnName, short? defaultValue)
    {
        var i = record.GetColumIndex(columnName, false);
        if (i < 0) return defaultValue;
        if (record.IsDBNull(i)) return defaultValue;
        var value = record.GetValue(i);
        if (value is int b) return b;
        if (value is short a) return a;
        throw new InvalidOperationException(columnName);
    }


    public static double GetDouble(this IDataRecord record, string columnName, double defaultValue = 0)
    {
        var i = record.GetColumIndex(columnName);
        if (record.IsDBNull(i)) return defaultValue;
        var value = record.GetValue(i);
        if (value is double b) return b;
        if (value is float a) return a;
        throw new InvalidOperationException(columnName);
    }

    public static string GetTime(this IDataRecord record, string columnName, string defaultValue = "")
    {
        var i = record.GetColumIndex(columnName);
        if (record.IsDBNull(i)) return defaultValue;
        return record.GetDateTime(i).ToString("HH:mm", CultureInfo.InvariantCulture);
    }

    public static TimeSpan GetTimeAsTimespan(this IDataRecord record, string columnName)
    {
        var i = record.GetColumIndex(columnName);
        if (record.IsDBNull(i)) return TimeSpan.MinValue;
        var value = record.GetValue(i);
        if (value is DateTime d) return new TimeSpan(d.Hour, d.Minute, 0);
        throw new InvalidOperationException(columnName);
    }

    public static double GetTimeAsDouble(this IDataRecord record, string columnName)
    {
        var t = record.GetTimeAsTimespan(columnName);
        return t.TotalMinutes;
    }

    public static bool GetBool(this IDataRecord record, string columnName)
    {
        var i = record.GetColumIndex(columnName);
        if (record.IsDBNull(i)) return false;
        var value = record.GetValue(i);
        if (value is bool a) return a;
        if (value is Int16 b) return b != 0;
        if (value is Int32 c) return c != 0;
        if (value is double d) return d != 0;
        throw new InvalidOperationException(columnName);
    }

    public static bool IsDBNull(this IDataRecord record, string columnName)
    {
        var i = record.GetOrdinal(columnName);
        return record.IsDBNull(i);
    }

    private static int GetColumIndex(this IDataRecord record, string columnName, bool throwOnNotFound = true)
    {
        int i;
        try { i = record.GetOrdinal(columnName); }
        catch (IndexOutOfRangeException)
        {
            if (throwOnNotFound || ThrowOnMissingColumnName) throw new InvalidOperationException(columnName);
        }
        return i;
    }
}
