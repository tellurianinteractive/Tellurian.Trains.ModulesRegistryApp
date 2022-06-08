using System.Text;

namespace ModulesRegistry.Services.Extensions;
public static class LocoAddressExtensions
{
    public static int[] AsLocoAdresses(this string? values)
    {
        if (string.IsNullOrWhiteSpace(values)) return Array.Empty<int>();
        var itemGroups = values.Split(',');
        if (itemGroups.Length == 0) return Array.Empty<int>();
        var result = new List<int>();
        foreach (var group in itemGroups)
        {
            if (string.IsNullOrWhiteSpace(group)) continue;
            var trimmedGroup = group.Trim();
            var interval = trimmedGroup.Split('-');
            if (interval.Length == 0) continue;
            if (interval.Length == 1 && int.TryParse(interval[0], out int address) && IsValidDccAddress(address)) result.Add(address);
            if (interval.Length == 2 && int.TryParse(interval[0], out int fromAddress) && int.TryParse(interval[1], out int toAddress) && fromAddress < toAddress && IsValidDccAddress(fromAddress) && IsValidDccAddress(toAddress)) result.AddRange(Enumerable.Range(fromAddress, toAddress - fromAddress + 1));
            if (interval.Length > 2) throw new ArgumentOutOfRangeException(nameof(values), values);
        }
        return result.ToArray();

        static bool IsValidDccAddress(int address) => address >= 1 && address <= 9999;
    }

    public static string AsCollapsedLocoAdresses(this int[]? adresses)
    {
        if (adresses is null || adresses.Length == 1) return string.Empty;
        var result = new StringBuilder(200);
        int intervalStartIndex = -1;
        var orderedAdresses = adresses.OrderBy(a => a).ToArray();
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
            //intervalStartIndex = -1;
        }
        return result.ToString();
    }

}
