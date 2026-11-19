namespace ModulesRegistry.Data;

/// <summary>
/// A period of years (decided 2026-11-19, Migration step 3.5), such as from/upto, opened/closed or first/last year. Both years are
/// optional: an empty <see cref="From"/> is open backwards, an empty <see cref="Upto"/> open forwards, and both empty means no time
/// limit. <see cref="Upto"/> is <b>inclusive</b>, as users assume, and as the SQL views and procedures compare (<c>&lt;=</c> and
/// <c>&gt;=</c>, open ends as 0 and 9999). A period is valid when <see cref="From"/> is not after <see cref="Upto"/>; equal years
/// are one year.
/// </summary>
public readonly record struct YearPeriod(int? From, int? Upto)
{
    /// <summary>The earliest year accepted in a form; a vehicle's manufacture year may be earlier (<see cref="MinVehicleYear"/>).</summary>
    public const int MinYear = 1900;
    public const int MinVehicleYear = 1850;
    /// <summary>The latest year accepted in a form: a fixed limit, so the built-in range validation can check it in the browser.</summary>
    public const int MaxYear = 2100;

    public YearPeriod(short? from, short? upto) : this((int?)from, upto) { }

    public bool IsUnlimited => From is null && Upto is null;

    /// <summary>From is not after upto; an open end is always valid.</summary>
    public bool IsValid => From is null || Upto is null || From <= Upto;

    public bool Contains(int year) => (From is null || From <= year) && (Upto is null || year <= Upto);

    /// <summary>The periods have at least one year in common; an open end overlaps everything on its side.</summary>
    public bool Overlaps(YearPeriod other) =>
        (From is null || other.Upto is null || From <= other.Upto) &&
        (Upto is null || other.From is null || Upto >= other.From);

    /// <summary>Corrected when saving: a year of zero or less is not given, and reversed years are swapped.</summary>
    public YearPeriod AsCorrected()
    {
        var (from, upto) = (From > 0 ? From : null, Upto > 0 ? Upto : null);
        return from > upto ? new(upto, from) : new(from, upto);
    }

    /// <summary>The corrected period of two stored years (<see cref="AsCorrected"/>), to assign back: <c>(e.From, e.Upto) = YearPeriod.Corrected(e.From, e.Upto)</c>.</summary>
    public static (short? From, short? Upto) Corrected(short? from, short? upto)
    {
        var corrected = new YearPeriod(from, upto).AsCorrected();
        return (corrected.FromAsShort, corrected.UptoAsShort);
    }

    public short? FromAsShort => (short?)From;
    public short? UptoAsShort => (short?)Upto;

    /// <summary>"1950–1965", "1950–", "–1965", or empty for no limit.</summary>
    public override string ToString() => IsUnlimited ? string.Empty : $"{From}–{Upto}";
}