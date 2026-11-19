namespace ModulesRegistry.Data.Extensions;

/// <summary>
/// Loco addresses: DCC addresses from <see cref="MinDccAddress"/> to <see cref="MaxDccAddress"/>, written as numbers and ranges
/// such as <c>3,10-15</c> (the number ranges of Tellurian.Utilities.Text).
/// </summary>
public static class LocoAddressExtensions
{
    public const int MinDccAddress = 1;
    public const int MaxDccAddress = 9999;

    extension(string? text)
    {
        /// <summary>Corrected when saving: separators, reversed ranges, duplicates, order (<c>AsNumberRanges</c>).</summary>
        public string? AsLocoAddresses() => text.AsNumberRanges(MinDccAddress, MaxDccAddress);

        public bool TryParseLocoAdresses(out int[] addresses) => text.TryParseNumberRanges(MinDccAddress, MaxDccAddress, out addresses);
    }
}