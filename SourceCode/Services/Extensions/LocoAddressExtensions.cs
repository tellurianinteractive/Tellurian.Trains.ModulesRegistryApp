using System.Text;

namespace ModulesRegistry.Services.Extensions;
public static class LocoAddressExtensions
{
    public static bool TryParseLocoAdresses(this string? values, out int[] addresses)
    {
        addresses = Array.Empty<int>();
        if (string.IsNullOrWhiteSpace(values)) return true;
        var itemGroups = values.Split(',');
        if (itemGroups.Length == 0) return true;
        var result = new List<int>();
        foreach (var group in itemGroups)
        {
            if (string.IsNullOrWhiteSpace(group)) continue;
            var trimmedGroup = group.Trim();
            var interval = trimmedGroup.Split('-');
            if (interval.Length == 0) continue;
            if (interval.Length == 1)
            {
                if (int.TryParse(interval[0], out int address) && address.IsValidDccAddress())
                    result.Add(address);
                else
                    return false;
            }
            if (interval.Length == 2)
            {
                if (int.TryParse(interval[0], out int fromAddress) && int.TryParse(interval[1], out int toAddress) && fromAddress < toAddress && fromAddress.IsValidDccAddress() && toAddress.IsValidDccAddress())
                    result.AddRange(Enumerable.Range(fromAddress, toAddress - fromAddress + 1));
                else
                    return false;
            }
            if (interval.Length > 2) return false;
        }
        addresses = result.ToArray();
        return true;

    }

    public static string AsCollapsedLocoAdresses(this int[]? adresses)
    {
        if (adresses is null || adresses.Length == 1) return string.Empty;
        var result = new StringBuilder(200);
        int intervalStartIndex = -1;
        var orderedAdresses = adresses.Where(a => a.IsValidDccAddress()).OrderBy(a => a).ToArray();
        for (var i = 0; i < orderedAdresses.Length; i++)
        {
            if (i < orderedAdresses.Length - 1 && orderedAdresses[i] + 1 == orderedAdresses[i + 1])
            {
                if (intervalStartIndex == -1) intervalStartIndex = i;
            }
            else
            {
                if (intervalStartIndex >= 0) { result.Append(orderedAdresses[intervalStartIndex]); result.Append('-'); intervalStartIndex = -1; }
                result.Append(orderedAdresses[i]);
                if (i < orderedAdresses.Length - 1) result.Append(',');

            }
        }
        return result.ToString();
    }
    private static bool IsValidDccAddress(this int address) => address >= 1 && address <= 9999;

}
