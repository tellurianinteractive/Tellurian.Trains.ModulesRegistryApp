using ModulesRegistry.Services.Resources;
using System;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;

namespace ModulesRegistry.Services.Extensions
{
    public class OperationDays
    {
        public string FullName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public bool IsDaily { get; set; }

        public override bool Equals(object? obj) => obj is OperationDays other && other.ShortName.Equals(ShortName, StringComparison.OrdinalIgnoreCase);
        public override int GetHashCode() => ShortName.GetHashCode(StringComparison.OrdinalIgnoreCase);
        public override string ToString() => ShortName;
    }

    public static class OperationDaysExtensions
    {
        private static readonly Day[] Days = new[] {
            new Day(0, 0x7F, "Daily"),
            new Day(1, 0x01, "Monday"),
            new Day(2, 0x02, "Tuesday"),
            new Day(3, 0x04, "Wednesday"),
            new Day(4, 0x08, "Thursday"),
            new Day(5, 0x10, "Friday"),
            new Day(6, 0x20, "Saturday"),
            new Day(7, 0x40, "Sunday"),
            new Day(0, 0x80, "OnDemand") };

        private static Day[] GetDays(this byte flags) =>
            flags == Days[0].Flag ? new Day[] { Days[0] } :
            flags == Days[8].Flag ? new Day[] { Days[8] } :
            Days.Where(d => d.Number > 0 && (d.Flag & flags) > 0).ToArray();

        public static int DisplayOrder(this byte flags) => ~flags;

        public static byte And(this byte flags, byte and) => (byte)(flags & and);

        public static OperationDays OperationDays(this byte flags, CultureInfo? culture = null)
        {
            if (culture is null) culture = CultureInfo.CurrentCulture;
            var days = GetDays(flags);
            var isDaily = flags == 0x7F;
            var fullName = new StringBuilder(20);
            var shortName = new StringBuilder(10);
            var to = Strings.ResourceManager.GetString("In", culture) ?? "to";
            var and = Strings.ResourceManager.GetString("And", culture) ?? "and";
            if (days.Length == 1)
            {
                fullName.Append(days[0].FullName(culture));
                shortName.Append(days[0].ShortName(culture));
            }
            else
            {
                var dayNumber = 0;
                var lastDayNumber = days.Last().Number;
                if (days.IsConsectutive())
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

            fullNames.Append(toLower && Strings.DayNameCasing.Equals("LOWER", StringComparison.OrdinalIgnoreCase) ? day.FullName(culture).ToLowerInvariant() : day.FullName(culture));
            shortNames.Append(day.ShortName(culture));
        }
        public static void Append(this string fullText, string shortText, StringBuilder fullNames, StringBuilder shortNames)
        {
            if (fullText.Length > 1) fullNames.Append(' ');
            fullNames.Append(fullText);
            fullNames.Append(' ');
            shortNames.Append(shortText);
        }
    }

    internal class Day
    {
        public Day(byte number, byte flag, string resourceKey)
        {
            Number = number;
            Flag = flag;
            FullNameResourceKey = resourceKey;
            ShortNameResourceKey = resourceKey + "Short";
        }
        public byte Flag { get; }
        public byte Number { get; }
        private string FullNameResourceKey { get; }
        private string ShortNameResourceKey { get; }
        public string ShortName(CultureInfo culture)
        {
            var resourceManager = Strings.ResourceManager;
            if (resourceManager == null) return string.Empty;
            return resourceManager.GetString(ShortNameResourceKey, culture) ?? string.Empty;
        }

        public string FullName(CultureInfo culture)
        {
            var resourceManager = Strings.ResourceManager;
            if (resourceManager == null) return string.Empty;
            return resourceManager.GetString(FullNameResourceKey, culture) ?? string.Empty;
        }

    }

    internal static class DayExtensions
    {
        public static bool IsConsectutive(this Day[] days) => days.Length == days.Last().Number - days[0].Number + 1;
    }
}
