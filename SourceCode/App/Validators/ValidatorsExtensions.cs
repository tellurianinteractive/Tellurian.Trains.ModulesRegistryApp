using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Services.Extensions;
using System;

namespace ModulesRegistry.Validators
{
    public static class ValidatorsExtensions
    {
        public static IRuleBuilderOptions<T, string> MustBeCapitalizedCorrectly<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
            builder.Must(value => value.IsNameCapitalizedCorrectly()).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeginWithCapitalLetter"]}");


        private static bool IsNameCapitalizedCorrectly(this string? name)
        {
            if (name is null || name.Length < 1) return true;
            var n = name.AsSpan();
            for (var i = 0; i < n.Length; i++)
            {
                if (i == 0)
                {
                    if (char.IsLower(n[i]))
                        return false;
                }
                else if ((n[i - 1] == ' '))
                {
                    var rest = n[i..];
                    if (IsAnyLowerCaseWord(rest)) continue;
                    if (char.IsLower(n[i])) return false;
                }
            }
            return true;
        }

        private static bool IsAnyLowerCaseWord(ReadOnlySpan<char> word)
        {
            foreach (var w in LowerCaseWords)
            {
                if (w.Length > word.Length) continue;
                if (MemoryExtensions.StartsWith(word, w, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static string[] LowerCaseWords => new[] { "af", "am", "an", "auf", "och", "und", "von" };


        public static IRuleBuilderOptions<T, string?> MustBeOrdinaryTextOrNull<T>(this IRuleBuilder<T, string?> builder, IStringLocalizer localizer) =>
           builder.Must(value => value.IsText()).WithMessage($"\"{{PropertyName}}\" {localizer["MayOnlyContainOrdinaryText"]}");

        public static IRuleBuilderOptions<T, string> MustBeOrdinaryText<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
            builder.Must(value => value.IsText()).WithMessage($"\"{{PropertyName}}\" {localizer["MayOnlyContainOrdinaryText"]}");
        private static bool IsText(this string? text)
        {
            if (string.IsNullOrEmpty(text)) return true;
            foreach (var c in text)
            {
                if (c.IsLatinChar()) continue;
                if (c.IsDigit()) continue;
                if (c.IsPermittedPunctuationOrSymbol()) continue;
                return false;
            }
            return true;
        }

        private static bool IsInRange(this char c, int firstCodePoint, int lastCodePoint) =>
            c >= firstCodePoint && c <= lastCodePoint;
        private static bool IsPermittedPunctuationOrSymbol(this char c) => " (),.:;-+*!?%&§#±°²³/«»£€´".Contains(c);
        private static bool IsDigit(this char c) => c >= 0x0030 && c <= 0x0039;
        private static bool IsLatinChar(this char c) =>
            c.IsInRange(0x0061, 0x007A) ||
            c.IsInRange(0x0041, 0x005A) ||
            c.IsInRange(0x00C0, 0x00D6) ||
            c.IsInRange(0x00D8, 0X00F6) ||
            c.IsInRange(0x00F8, 0x00FF) ||
            c.IsInRange(0x0104, 0x0107) ||
            c.IsInRange(0x0118, 0x0119) ||
            c.IsInRange(0x0141, 0x0144) ||
            c.IsInRange(0x015A, 0x015B) ||
            c.IsInRange(0x0179, 0x017C);

        public static IRuleBuilderOptions<T, int> MustBeSelected<T>(this IRuleBuilder<T, int> builder, IStringLocalizer localizer, bool orZero = false) =>
            builder.Must(value => value.IsSelected(orZero)).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeSelected"]}");
        private static bool IsSelected(this int id, bool zero) => id > 0 || zero;

        public static IRuleBuilderOptions<T, int> MustBeEnumValue<T>(this IRuleBuilder<T, int> builder, IStringLocalizer localizer, Type enumType) =>
            builder.Must(value => value.IsValidEnum(enumType)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeAnyOf"].Value, string.Join(",", EnumExtensions.StationTrackDirections()))}");
        private static bool IsValidEnum(this int value, Type enumType) => Enum.IsDefined(enumType, value);

        public static IRuleBuilderOptions<T, short?> MustBeValidYear<T>(this IRuleBuilder<T, short?> builder, IStringLocalizer localizer) =>
            builder.Must(value => value.IsValidYear()).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetween"].Value, MinYear, MaxYear)}");
        private static bool IsValidYear(this short? year) => year is null || (year >= MinYear && year <= MaxYear);
        private static int MinYear => 1900;
        private static int MaxYear => DateTimeOffset.Now.Year;

        public static IRuleBuilderOptions<T, short?> MustBeValidHour<T>(this IRuleBuilder<T, short?> builder, IStringLocalizer localizer) =>
            builder.Must(value => value.IsValidHour()).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetween"].Value, MinHour, MaxHour)}");

        private static bool IsValidHour(this short? hour) => hour is null || (hour >= MinHour && hour <= MaxHour);
        const short MinHour = 0;
        const short MaxHour = 23;

        public static IRuleBuilderOptions<T, string?> MustBeColor<T>(this IRuleBuilder<T, string?> builder, IStringLocalizer localizer) =>
             builder.Must(value => value.IsHexColorOrNull()).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeAColor"].Value, MinHour, MaxHour)}");

        private static bool IsHexColorOrNull(this string? value) =>
            value is null || value.IsHexColor();
    }
}
