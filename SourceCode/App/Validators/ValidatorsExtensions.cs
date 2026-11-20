using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Forms.Validation;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Validators;

/// <summary>
/// Rules of the interactive forms' FluentValidation validators, until each form moves to static SSR with DataAnnotations
/// (Migration step 3.6). Since step 3.5 (decided 2026-11-19), what the service corrects when saving
/// (<c>CorrectInput</c> in Data's <see cref="EntityCorrectionExtensions"/>) is not validated: names aren't checked for capitals,
/// and the address, year, MAC address and colour rules check the value as it will be after the correction. Ordinary text is
/// still checked as typed, until each form's service also corrects it (step 3.6).
/// </summary>
public static partial class ValidatorsExtensions
{
    public static IRuleBuilderOptions<T, string?> MustBeOrdinaryTextOrNull<T>(this IRuleBuilder<T, string?> builder, IStringLocalizer localizer) =>
       builder.Must(value => value.IsOrdinaryText()).WithMessage($"\"{{PropertyName}}\" {localizer["MustOnlyContainOrdinaryText"]}");

    public static IRuleBuilderOptions<T, string> MustBeOrdinaryText<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
        builder.Must(value => value.IsOrdinaryText()).WithMessage($"\"{{PropertyName}}\" {localizer["MustOnlyContainOrdinaryText"]}");

    /// <summary>Loco addresses, after the correction when saving (<see cref="LocoAddressExtensions.AsLocoAddresses"/>).</summary>
    public static IRuleBuilderOptions<T, string> MustBeLocoAdresses<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
        builder.Must(value => value.IsLocoAddressesWhenCorrected()).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeValidDccAdresses"]}");

    /// <summary>Email addresses, after the correction when saving (<c>AsEmailAddresses</c>).</summary>
    public static IRuleBuilderOptions<T, string> MustBeValidEmailAdresses<T>(this IRuleBuilder<T, string> builder, IStringLocalizer localizer) =>
        builder.Must(value => value.IsEmailAddressesWhenCorrected()).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeValidEmailAddresses"]}");

    public static IRuleBuilderOptions<T, Station> IsRegionRequired<T>(this IRuleBuilder<T, Station> builder, IStringLocalizer localizer) =>
        builder.Must(x => x.IsShadow || x.RegionId.HasValue).WithMessage(value => $"\"{{PropertyName}}\" {localizer["MustBeSelected"]}");

    // Zero means "not given" and is saved as empty (AsNullIfZero when saving).
    public static IRuleBuilderOptions<T, double?> IsEmptyOrInclusiveBetween<T>(this IRuleBuilder<T, double?> builder, double min, double max, IStringLocalizer localizer) =>
        builder.Must(d => d.HasValue == false || d.Value == 0 || (d.Value >= min && d.Value <= max)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetween"], min, max)}");

    public static IRuleBuilderOptions<T, decimal?> IsEmptyOrInclusiveBetween<T>(this IRuleBuilder<T, decimal?> builder, decimal min, decimal max, IStringLocalizer localizer) =>
        builder.Must(d => d.HasValue == false || d.Value == 0 || (d.Value >= min && d.Value <= max)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetween"], min, max)}");

    public static IRuleBuilderOptions<T, short?> IsEmptyOrInclusiveBetween<T>(this IRuleBuilder<T, short?> builder, int min, int max, IStringLocalizer localizer) =>
        builder.Must(d => d.HasValue == false || d.Value == 0 || (d.Value >= min && d.Value <= max)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetween"], min, max)}");

    public static IRuleBuilderOptions<T, int> MustBeSelected<T>(this IRuleBuilder<T, int> builder, IStringLocalizer localizer, bool orZero = false) =>
        builder.Must(value => value > 0 || orZero).WithMessage($"\"{{PropertyName}}\" {localizer["MustBeSelected"]}");

    public static IRuleBuilderOptions<T, int> MustBeEnumValue<T>(this IRuleBuilder<T, int> builder, IStringLocalizer localizer, Type enumType) =>
        builder.Must(value => Enum.IsDefined(enumType, value)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeAnyOf"].Value, string.Join(",", EnumExtensions.StationTrackDirections()))}");

    /// <summary>
    /// A year of a period (<see cref="YearPeriod"/>), or empty: between <paramref name="minimum"/> and <see cref="YearPeriod.MaxYear"/>.
    /// Zero or less is accepted, since it is saved as empty; reversed years are swapped when saving, so they are accepted too.
    /// </summary>
    public static IRuleBuilderOptions<T, short?> MustBeYearOrEmpty<T>(this IRuleBuilder<T, short?> builder, IStringLocalizer localizer, int minimum = YearPeriod.MinYear) =>
        builder.Must(year => year.IsYearOrEmpty(minimum)).WithMessage($"\"{{PropertyName}}\" {string.Format(localizer["MustBeBetweenOrEmpty"].Value, minimum, YearPeriod.MaxYear)}");

    extension(string? value)
    {
        internal bool IsEmailAddressesWhenCorrected() =>
            value.AsEmailAddresses().ToItems().All(EmailRegex().IsMatch);

        internal bool IsLocoAddressesWhenCorrected() => value.AsLocoAddresses().TryParseLocoAdresses(out _);
    }

    extension(short? year)
    {
        internal bool IsYearOrEmpty(int minimum) => year is null || year <= 0 || (year >= minimum && year <= YearPeriod.MaxYear);
    }

    [GeneratedRegex("^[a-zA-Z0-9.!#$%&'*+\\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$")]
    private static partial Regex EmailRegex();

}