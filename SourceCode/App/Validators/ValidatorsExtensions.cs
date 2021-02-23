using FluentValidation;
using Microsoft.Extensions.Localization;
using System;

namespace ModulesRegistry.Validators
{
    public static class ValidatorsExtensions
    {
        public static IRuleBuilderOptions<T, string> NameIsCapitalizedCorrectly<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
            builder.Must(value => value.IsNameCapitalizedCorrectly()).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeginWithCapitalLetter"]}");

        private static bool IsNameCapitalizedCorrectly(this string? name)
        {
            if (name is null || name.Length < 1) return true;
            for (var i = 0; i < name.Length; i++)
            {
                if (i == 0 || (name[i - 1] == ' '))
                {
                    if (Char.IsLower(name[i])) return false;
                }
            }
            return true;
        }

        public static IRuleBuilderOptions<T, int> MustBeSelected<T>(this IRuleBuilder<T, int> builder, IStringLocalizer localizer) =>
            builder.Must(value => value.IsSelected()).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeSelected"]}");

        private static bool IsSelected(this int id) => id > 0;

        public static IRuleBuilderOptions<T, short?> MustBeValidYear<T>(this IRuleBuilder<T, short?> builder, IStringLocalizer localizer) =>
            builder.Must(value => value.IsValidYear()).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeYearBetween"].Value, MinYear, MaxYear)}");
        private static bool IsValidYear(this short? year) => year is null || (year >= MinYear && year <= MaxYear);
        private static int MinYear => 1900;
        private static int MaxYear => DateTimeOffset.Now.Year;


    }
}
