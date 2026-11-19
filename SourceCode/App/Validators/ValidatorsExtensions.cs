using FluentValidation;
using FluentValidation.Validators;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Forms.Validation;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Validators;

public static partial class ValidatorsExtensions
{

    public static IRuleBuilderOptions<T, string> MustBeCapitalizedCorrectly<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer, bool allWordsWithInitialCapitals = true) =>
        builder.MustBeCapitalizedCorrectly(localizer, allWordsWithInitialCapitals ? NameStyle.AllWords : NameStyle.FirstLetter);

    // The same rules as the correction when saving (NameCapitalizationExtensions): valid when the correction wouldn't change the name.
    public static IRuleBuilderOptions<T, string> MustBeCapitalizedCorrectly<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer, NameStyle style) =>
        builder.Must(value => value.IsCapitalizedName(style)).WithMessage(Message(localizer, style != NameStyle.FirstLetter));



    private static string Message(IStringLocalizer localizer, bool allWordsWithInitialCapitals) =>
        allWordsWithInitialCapitals ?
        $"\"{{PropertyName}}\" {localizer["AllWordsMustBeginWithCapitalLetter"]}" :
        $"\"{{PropertyName}}\" {localizer["MustBeginWithCapitalLetter"]}";


    public static IRuleBuilderOptions<T, string?> MustBeOrdinaryTextOrNull<T>(this IRuleBuilder<T, string?> builder, IStringLocalizer localizer) =>
       builder.Must(value => value.IsOrdinaryText()).WithMessage($"\"{{PropertyName}}\" {localizer["MustOnlyContainOrdinaryText"]}");

    public static IRuleBuilderOptions<T, string> MustBeOrdinaryText<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
        builder.Must(value => value.IsOrdinaryText()).WithMessage($"\"{{PropertyName}}\" {localizer["MustOnlyContainOrdinaryText"]}");

    public static IRuleBuilderOptions<T, string> MustBeLocoAdresses<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
        builder.Must(value => value.TryParseLocoAdresses(out var _)).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeValidDccAdresses"]}");

    public static IRuleBuilderOptions<T, string> MustBeValidEmailAdresses<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
     builder.Must(value => string.IsNullOrEmpty(value) || value.Split(';').All(email => email.IsValidEmailAddress())).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeValidEmailAddresses"]}");

    public static IRuleBuilderOptions<T, Station> IsRegionRequired<T>(this IRuleBuilder<T, Station> builder, IStringLocalizer localizer) =>
        builder.Must(x => x.IsShadow || x.RegionId.HasValue).WithMessage(value => $"\"{{PropertyName}}\" {localizer["MustBeSelected"]}");
    public static IRuleBuilderOptions<T, double?> IsEmptyOrInclusiveBetween<T>(this IRuleBuilder<T, double?> builder, double min, double max, IStringLocalizer localizer) =>
    builder.Must(d => d.HasValue == false || (min > 0 && d.Value == 0) || (d.Value >= min && d.Value <= max)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetween"],min,max)}");

    public static IRuleBuilderOptions<T, decimal?> IsEmptyOrInclusiveBetween<T>(this IRuleBuilder<T, decimal?> builder, decimal min, decimal max, IStringLocalizer localizer) =>
        builder.Must(d => d.HasValue == false || (min > 0 && d.Value == 0) || (d.Value >= min && d.Value <= max)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetween"], min, max)}");
    public static IRuleBuilderOptions<T, short?> IsEmptyOrInclusiveBetween<T>(this IRuleBuilder<T, short?> builder, int min, int max, IStringLocalizer localizer) =>
        builder.Must(d => d.HasValue == false || (min > 0 && d.Value == 0) || (d.Value >= min && d.Value <= max)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetween"], min, max)}");
    private static bool IsValidEmailAddress(this string? email)
    {
        if (email == null) return false;
        return EmailRegex().IsMatch(email);
    }
    private static bool IsMacAddress(this string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        if (text.Length != 17) return false;
        foreach (var c in text)
        {
            if (c.IsHexDigit()) continue;
            if (c == ':') continue;
            return false;
        }
        return true;
    }

    private static bool IsDccAddress(this short? value) => value.HasValue && value.Value >= 1 && value.Value <= 9999;

    private static bool IsDigit(this char c) => c.IsInRange(0x0030, 0x0039);
    private static bool IsHexDigit(this char c) => c.IsDigit() || c.IsInRange(0x0041, 0x0046) || c.IsInRange(0x0061, 0x066);
    private static bool IsInRange(this char c, int firstCodePoint, int lastCodePoint) =>
        c >= firstCodePoint && c <= lastCodePoint;


    public static IRuleBuilderOptions<T, int> MustBeSelected<T>(this IRuleBuilder<T, int> builder, IStringLocalizer localizer, bool orZero = false) =>
        builder.Must(value => value.IsSelected(orZero)).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeSelected"]}");
    private static bool IsSelected(this int id, bool zero) => id > 0 || zero;

    public static IRuleBuilderOptions<T, int> MustBeEnumValue<T>(this IRuleBuilder<T, int> builder, IStringLocalizer localizer, Type enumType) =>
        builder.Must(value => value.IsValidEnum(enumType)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeAnyOf"].Value, string.Join(",", EnumExtensions.StationTrackDirections()))}");
    private static bool IsValidEnum(this int value, Type enumType) => Enum.IsDefined(enumType, value);

    public static IRuleBuilderOptions<T, short?> MustBeValidYear<T>(this IRuleBuilder<T, short?> builder, IStringLocalizer localizer) =>
        builder.Must(value => value.IsValidYear()).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetweenOrEmpty"].Value, MinYear, MaxYear)}");
    private static bool IsValidYear(this short? year) => year is null || year >= MinYear && year <= MaxYear;
    private static int MinYear => 1900;
    private static int MaxYear => DateTimeOffset.Now.Year;


    public static IRuleBuilderOptions<T, string?> MustBeColor<T>(this IRuleBuilder<T, string?> builder, IStringLocalizer localizer) =>
         builder.Must(value => value.IsHexColorOrNull()).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeAColor"].Value}");

    public static IRuleBuilderOptions<T, string?> MustBeMacAddress<T>(this IRuleBuilder<T, string?> builder, IStringLocalizer localizer) =>
        builder.Must(value => value.IsMacAddress()).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeAMacAddress"].Value}");
    public static IRuleBuilderOptions<T, short?> MustBeDccAddressOrEmpty<T>(this IRuleBuilder<T, short?> builder, IStringLocalizer localizer) =>
        builder.Must(value => value is null || value.IsDccAddress()).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeADccAddress"].Value}");
    private static bool IsHexColorOrNull(this string? value) =>
        value is null || value.IsHexColor();

    [GeneratedRegex("^[a-zA-Z0-9.!#$%&'*+\\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$")]
    private static partial Regex EmailRegex();
}
