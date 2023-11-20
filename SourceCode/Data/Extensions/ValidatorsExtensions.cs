using FluentValidation;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Data.Extensions;

public static partial class ValidatorsExtensions
{

    public static IRuleBuilderOptions<T, string> MustBeCapitalizedCorrectly<T>(this IRuleBuilder<T, string> builder, bool allWordsWithInitialCapitals = true) =>
        builder.Must(value => value.IsNameCapitalizedCorrectly(allWordsWithInitialCapitals)).WithMessage(Message(allWordsWithInitialCapitals));



    private static string Message(bool allWordsWithInitialCapitals) =>
        allWordsWithInitialCapitals ?
        $"\"{{PropertyName}}\" {Resources.Validators.MustBeginWithCapitalLetter}" :
        $"\"{{PropertyName}}\" {Resources.Validators.AllWordsMustBeginWithCapitalLetter}";


    private static bool IsNameCapitalizedCorrectly(this string? name, bool allWordsWithInitialCapitals)
    {
        if (name is null || name.Length < 1) return true;
        var n = name.AsSpan();
        if (allWordsWithInitialCapitals)
        {
            for (var i = 0; i < n.Length; i++)
            {
                if (i == 0)
                {
                    if (char.IsLower(n[i]))
                    {
                        if (IsAnyLowerCaseWord(n)) continue;
                        return false;
                    }
                }
                else if (n[i - 1] == ' ')
                {
                    var rest = n[i..];
                    if (IsAnyLowerCaseWord(rest)) continue;
                    if (char.IsLower(n[i])) return false;
                }
            }
            return true;
        }
        else
        {
            return !char.IsLower(n[0]);
        }
    }

    private static bool IsAnyLowerCaseWord(ReadOnlySpan<char> word)
    {
        foreach (var w in LowerCaseWords)
        {
            if (w.Length > word.Length) continue;
            if (word.StartsWith(w, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static string[] LowerCaseWords => new[] { "i", "af", "am", "an", "by", "in", "im", "auf", "och", "und", "van", "von" };

    public static IRuleBuilderOptions<T, string?> MustBeOrdinaryTextOrNull<T>(this IRuleBuilder<T, string?> builder) =>
       builder.Must(value => value.IsText()).WithMessage($"\"{{PropertyName}}\" {Resources.Validators.MustOnlyContainOrdinaryText}");

    public static IRuleBuilderOptions<T, string> MustBeOrdinaryText<T>(this IRuleBuilder<T, string> builder) =>
        builder.Must(value => value.IsText()).WithMessage($"\"{{PropertyName}}\" {Resources.Validators.MustOnlyContainOrdinaryText}");

    public static IRuleBuilderOptions<T, string> MustBeLocoAdresses<T>(this IRuleBuilder<T, string> builder) =>
        builder.Must(value => value.TryParseLocoAdresses(out var _)).WithMessage($"\"{{PropertyName}}\" {Resources.Validators.MustBeValidDccAdresses}");

    public static IRuleBuilderOptions<T, string> MustBeValidEmailAdresses<T>(this IRuleBuilder<T, string> builder) =>
     builder.Must(value => string.IsNullOrEmpty(value) || value.Split(';').All(email => email.IsValidEmailAddress())).WithMessage($"\"{{PropertyName}}\" {Resources.Validators.MustBeValidEmailAddresses}");

    public static IRuleBuilderOptions<T, Station> IsRegionRequired<T>(this IRuleBuilder<T, Station> builder) =>
        builder.Must(x => x.IsShadow || x.RegionId.HasValue).WithMessage(value => $"\"{{PropertyName}}\" {Resources.Validators.MustBeSelected}");

    private static bool IsValidEmailAddress(this string? email)
    {
        if (email == null) return false;
        return EmailRegex().IsMatch(email);
    }
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
    private static bool IsMacAddress(this string? text)
    {
        if (string.IsNullOrEmpty (text)) return false;
        if (text.Length != 17) return false;
        foreach(var c in text)
        {
            if (c.IsHexDigit()) continue;
            if (c == ':') continue;
            return false;
        }
        return true;
    }

    private static bool IsDccAddress(this short? value) => value.HasValue && value.Value >= 1 && value.Value <= 9999;

    private static bool IsPermittedPunctuationOrSymbol(this char c) => " (),.:;-+*!?%&§#±°²³/«»£€´'\"".Contains(c);
    private static bool IsDigit(this char c) =>  c.IsInRange(0x0030, 0x0039);
    private static bool IsHexDigit(this char c) => c.IsDigit() || c.IsInRange(0x0041, 0x0046) || c.IsInRange(0x0061, 0x066);
    private static bool IsLatinChar(this char c) =>
        c.IsInRange(0x0061, 0x007A) ||
        c.IsInRange(0x0041, 0x005A) ||
        c.IsInRange(0x00C0, 0x00FF) ||
        c.IsInRange(0x0100, 0x0148) ||
        c.IsInRange(0x014A, 0x017F) ||
        c.IsInRange(0x0100, 0x0148) ||
        c.IsInRange(0x014A, 0x01BF) ||
        c.IsInRange(0x1E00, 0x1E9B);
    private static bool IsInRange(this char c, int firstCodePoint, int lastCodePoint) =>
        c >= firstCodePoint && c <= lastCodePoint;


    public static IRuleBuilderOptions<T, int> MustBeSelected<T>(this IRuleBuilder<T, int> builder, bool orZero = false) =>
        builder.Must(value => value.IsSelected(orZero)).WithMessage($"\"{{PropertyName}}\" {Resources.Validators.MustBeSelected}");
    private static bool IsSelected(this int id, bool zero) => id > 0 || zero;

    public static IRuleBuilderOptions<T, int> MustBeEnumValue<T>(this IRuleBuilder<T, int> builder, Type enumType) =>
        builder.Must(value => value.IsValidEnum(enumType)).WithMessage($"\"{{PropertyName}}\" {string.Format(Resources.Validators.MustBeAnyOf, string.Join(",", EnumExtensions.StationTrackDirections()))}");
    private static bool IsValidEnum(this int value, Type enumType) => Enum.IsDefined(enumType, value);

    public static IRuleBuilderOptions<T, short?> MustBeValidYear<T>(this IRuleBuilder<T, short?> builder) =>
        builder.Must(value => value.IsValidYear()).WithMessage($"\"{{PropertyName}}\" {string.Format(Resources.Validators.MustBeBetween, MinYear, MaxYear)}");
    private static bool IsValidYear(this short? year) => year is null || year >= MinYear && year <= MaxYear;
    private static int MinYear => 1900;
    private static int MaxYear => DateTimeOffset.Now.Year;

    public static IRuleBuilderOptions<T, short?> MustBeValidHour<T>(this IRuleBuilder<T, short?> builder) =>
        builder.Must(value => value.IsValidHour()).WithMessage($"\"{{PropertyName}}\" {string.Format(Resources.Validators.MustBeBetween, MinHour, MaxHour)}");

    private static bool IsValidHour(this short? hour) => hour is null || hour >= MinHour && hour <= MaxHour;
    const short MinHour = 0;
    const short MaxHour = 23;

    public static IRuleBuilderOptions<T, string?> MustBeColor<T>(this IRuleBuilder<T, string?> builder) =>
         builder.Must(value => value.IsHexColorOrNull()).WithMessage($"\"{{PropertyName}}\" {string.Format(Resources.Validators.MustBeAColor)}");

    public static IRuleBuilderOptions<T, string?> MustBeMacAddress<T>(this IRuleBuilder<T, string?> builder) =>
        builder.Must(value => value.IsMacAddress()).WithMessage($"\"{{PropertyName}}\" {Resources.Validators.MustBeAMacAddress}");
    public static IRuleBuilderOptions<T, short?> MustBeDccAddressOrEmpty<T>(this IRuleBuilder<T, short?> builder) =>
        builder.Must(value => value is null || value.IsDccAddress()).WithMessage($"\"{{PropertyName}}\" {Resources.Validators.MustBeADccAddress}");
    private static bool IsHexColorOrNull(this string? value) =>
        value is null || value.IsHexColor();
    
    [GeneratedRegex("^[a-zA-Z0-9.!#$%&'*+\\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$")]
    private static partial Regex EmailRegex();
}
