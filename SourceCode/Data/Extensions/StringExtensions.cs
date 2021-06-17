using System;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Data.Extensions
{
    public static class StringExtensions
    {
        public static bool HasValue([NotNullWhen(true)] this string? me) =>
    !string.IsNullOrWhiteSpace(me);

        public static bool HasNoValue([NotNullWhen(false)] this string? me) =>
            string.IsNullOrWhiteSpace(me);
    }

    public static class DateTimeExtensions
    {
        public static string AsPeriod(this (DateTime? from, DateTime? to) period, string format = "d") =>
            period.from.HasValue && period.to.HasValue ? $"{period.from.Value.ToString(format)} - {period.to.Value.ToString(format)}" :
            period.from.HasValue ? $"{period.from.Value.ToString(format)} - " :
            period.to.HasValue ? $"- {period.to.Value.ToString(format)}" :
            string.Empty;

        public static string AsPeriod(this (short? from, short? to) period) =>
            period.from.HasValue && period.to.HasValue ? $"{period.from.Value} - {period.to.Value}" :
            period.from.HasValue ? $"{period.from.Value} - " :
            period.to.HasValue ? $"- {period.to.Value}" :
            string.Empty;

    }
}
