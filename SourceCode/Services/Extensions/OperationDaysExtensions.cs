﻿using ModulesRegistry.Services.Resources;
using System.Text;

namespace ModulesRegistry.Services.Extensions;

public class OperationDays
{
    public string FullName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsDaily { get; set; }

    public override bool Equals(object? obj) => obj is OperationDays other && other.ShortName.Equals(ShortName, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => ShortName.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => ShortName;
}

public static class OperatingDayExtensions
{
    public static string ShortNameLocalized(this OperatingDay day) =>
        day.Flag.OperationDays().ShortName;

    public static string FullNameLocalised(this OperatingDay day) =>
        day.Flag.OperationDays().FullName;
}

public static class OperationDaysExtensions
{
    private static readonly Day[] Days = [
            new Day(0, 0x7F, "Daily"),
            new Day(1, 0x01, "Monday"),
            new Day(2, 0x02, "Tuesday"),
            new Day(3, 0x04, "Wednesday"),
            new Day(4, 0x08, "Thursday"),
            new Day(5, 0x10, "Friday"),
            new Day(6, 0x20, "Saturday"),
            new Day(7, 0x40, "Sunday"),
            new Day(0, 0x80, "OnDemand") ];


    internal static Day[] GetDays(this byte flags, bool haveSundayFirst = false, bool expandDays = false) =>

        !expandDays && flags == Days[0].Flag ? new Day[] { Days[0] } :
        flags >= Days[8].Flag ? new Day[] { Days[8] } :
        haveSundayFirst ? Days.Where(d => d.Number == 7 && (d.Flag & flags) > 0).Concat(Days.Where(d => d.Number > 0 && d.Number < 7 && (d.Flag & flags) > 0)).ToArray() :
        Days.Where(d => d.Number > 0 && (d.Flag & flags) > 0).ToArray();

    public static int DisplayOrder(this byte flags) => ~flags;

    public static byte And(this byte flags, byte and) => (byte)(flags & and);

    public static OperationDays OperationDays(this byte flags, CultureInfo? culture = null) =>
        OperationDays(flags, false, culture);

    public static OperationDays OperationDays(this byte flags, bool isSundayFirst, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        var days = GetDays(flags, isSundayFirst);
        var isDaily = flags == 0x7F;
        var fullName = new StringBuilder(20);
        var shortName = new StringBuilder(10);
        var to = Strings.ResourceManager.GetString("In", culture) ?? "to";
        var and = Strings.ResourceManager.GetString("And", culture) ?? "and";
        if (days.Length == 1)
        {
            fullName.Append(days[0].GetLocalizedFullName(culture));
            shortName.Append(days[0].GetLocalizedShortName(culture));
        }
        else
        {
            var dayNumber = 0;
            var lastDayNumber = days.Last().Number;
            if (days.IsConsectutive(isSundayFirst))
            {
                Append(days[0], fullName, shortName, culture);
                Append(to, "-", fullName, shortName);
                Append(days[^1], fullName, shortName, culture, true);
            }
            else if (flags == 0x5F)
            {
                Append(Days[1], fullName, shortName, culture);
                Append(to, "-", fullName, shortName);
                Append(Days[5], fullName, shortName, culture, true);
                Append(and, ",", fullName, shortName);
                Append(Days[7], fullName, shortName, culture, true);
            }
            else if (flags == 0x4F)
            {
                Append(Days[7], fullName, shortName, culture);
                Append(to, "-", fullName, shortName);
                Append(Days[4], fullName, shortName, culture, true);
            }
            else
            {
                foreach (var day in days)
                {
                    if (day.Number == lastDayNumber)
                    {
                        Append(and, ",", fullName, shortName);
                    }
                    else if (dayNumber > 0)
                    {
                        Append(",", ",", fullName, shortName);
                    }
                    Append(day, fullName, shortName, culture, day.Number > days[0].Number);
                    dayNumber = day.Number;
                }
            }
        }
        return new OperationDays
        {
            IsDaily = isDaily,
            FullName = fullName.ToString(),
            ShortName = shortName.ToString()
        };
    }

    private static void Append(Day day, StringBuilder fullNames, StringBuilder shortNames, CultureInfo culture, bool toLower = false)
    {

        fullNames.Append(toLower && Strings.DayNameCasing.Equals("LOWER", StringComparison.OrdinalIgnoreCase) ? day.GetLocalizedFullName(culture).ToLowerInvariant() : day.GetLocalizedFullName(culture));
        shortNames.Append(day.GetLocalizedShortName(culture));
    }
    public static void Append(this string fullText, string shortText, StringBuilder fullNames, StringBuilder shortNames)
    {
        if (fullText.Length > 1) fullNames.Append(' ');
        fullNames.Append(fullText);
        fullNames.Append(' ');
        shortNames.Append(shortText);
    }
}

internal class Day(byte number, byte flag, string resourceKey)
{
    public byte Flag { get; } = flag;
    public byte Number { get; } = number;
    private string FullNameResourceKey { get; } = resourceKey;
    private string ShortNameResourceKey { get; } = resourceKey + "Short";
    public string GetLocalizedShortName(CultureInfo? specificCulture = null)
    {
        var resourceManager = Strings.ResourceManager;
        var culture = specificCulture ?? CultureInfo.CurrentCulture;
        if (resourceManager == null) return string.Empty;
        return resourceManager.GetString(ShortNameResourceKey, culture) ?? string.Empty;
    }

    public string GetLocalizedFullName(CultureInfo? specificCulture = null)
    {
        var resourceManager = Strings.ResourceManager;
        var culture = specificCulture ?? CultureInfo.CurrentCulture;
        if (resourceManager == null) return string.Empty;
        return resourceManager.GetString(FullNameResourceKey, culture) ?? string.Empty;
    }

    public override string ToString() => FullNameResourceKey;
}

internal static class DayExtensions
{
    public static bool IsConsectutive(this Day[] days, bool isSundaysFirst = false) =>
        isSundaysFirst && days[0].Number == 7 ? days.Length == days[^1].Number - days[1].Number + 2 :
        days.Length == days.Last().Number - days[0].Number + 1;
}
