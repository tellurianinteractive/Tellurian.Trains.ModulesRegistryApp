using System.Globalization;
using System.Text;
using ModulesRegistry.Services.Models;

namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// Renders <see cref="Waybill"/> objects as text, every property of the waybill and both ends, one block per waybill, sorted.
/// The waybill's <c>Id</c> is left out: it is a stored waybill's identity value, which isn't stable between tests.
/// </summary>
public static class WaybillText
{
    public static string Render(IEnumerable<Waybill> waybills)
    {
        var blocks = waybills.Select(Render).Order(StringComparer.Ordinal).ToList();
        return $"{blocks.Count} waybills\n\n{string.Join("\n", blocks)}";
    }

    private static string Render(Waybill w)
    {
        var text = new StringBuilder();
        Line(text, "OwnerStationId", w.OwnerStationId);
        Line(text, "OperatingDayFlag", w.OperatingDayFlag);
        Line(text, "OperatorName", w.OperatorName);
        Line(text, "Epoch", w.Epoch);
        Line(text, "DefaultWagonClass", w.DefaultWagonClass);
        Line(text, "SpecialWagonClass", w.SpecialWagonClass);
        Line(text, "Quantity", w.Quantity);
        Line(text, "QuantityUnitId", w.QuantityUnitId);
        Line(text, "QuantityShortUnit", w.QuantityShortUnit);
        Line(text, "QuantityIsBearer", w.QuantityIsBearer);
        Line(text, "HasEmptyReturn", w.HasEmptyReturn);
        Line(text, "HasSameCargoReturn", w.HasSameCargoReturn);
        Line(text, "IsExpress", w.IsExpress);
        Line(text, "IsCoolingRequired", w.IsCoolingRequired);
        Line(text, "MatchReturn", w.MatchReturn);
        Line(text, "PrintCount", w.PrintCount);
        Line(text, "PrintPerOperatingDay", w.PrintPerOperatingDay);
        Line(text, "IsEmptyReturn", w.IsEmptyReturn);
        Line(text, "HideLoadingTimes", w.HideLoadingTimes);
        Line(text, "HideUnloadingTimes", w.HideUnloadingTimes);
        Line(text, "IsLayoutInternal", w.IsLayoutInternal);
        Line(text, "OwnerNames", w.OwnerNames);
        Customer(text, "Origin", w.Origin);
        Customer(text, "Destination", w.Destination);
        return text.ToString();
    }

    private static void Customer(StringBuilder text, string end, CargoCustomer c)
    {
        Line(text, $"{end}.Id", c.Id);
        Line(text, $"{end}.Name", c.Name);
        Line(text, $"{end}.StationId", c.StationId);
        Line(text, $"{end}.StationName", c.StationName);
        Line(text, $"{end}.InternationalStationName", c.InternationalStationName);
        Line(text, $"{end}.IsModuleStation", c.IsModuleStation);
        Line(text, $"{end}.ForeColor", c.ForeColor);
        Line(text, $"{end}.BackColor", c.BackColor);
        Line(text, $"{end}.Languages", c.Languages);
        Line(text, $"{end}.DomainSuffix", c.DomainSuffix);
        Line(text, $"{end}.CargoName", c.CargoName);
        Line(text, $"{end}.SpecialCargoName", c.SpecialCargoName);
        Line(text, $"{end}.QuantityUnitResourceKey", c.QuantityUnitResourceKey);
        Line(text, $"{end}.PackagingUnitResourceKey", c.PackagingUnitResourceKey);
        Line(text, $"{end}.PackagingPrepositionResourceCode", c.PackagingPrepositionResourceCode);
        Line(text, $"{end}.ReadyTimeResourceKey", c.ReadyTimeResourceKey);
        Line(text, $"{end}.OperationDaysFlags", c.OperationDaysFlags);
        Line(text, $"{end}.DisplayReadyTime", c.DisplayReadyTime);
        Line(text, $"{end}.TrackOrArea", c.TrackOrArea);
        Line(text, $"{end}.TrackOrAreaColor", c.TrackOrAreaColor);
        Line(text, $"{end}.CargoTrackOrArea", c.CargoTrackOrArea);
        Line(text, $"{end}.CargoTrackOrAreaColor", c.CargoTrackOrAreaColor);
        Line(text, $"{end}.FromYear", c.FromYear);
        Line(text, $"{end}.UptoYear", c.UptoYear);
        Line(text, $"{end}.IsOrigin", c.IsOrigin);
        Line(text, $"{end}.Waybill is set", c.Waybill is not null);
    }

    private static void Line(StringBuilder text, string name, object? value) =>
        text.Append(name).Append(": ").Append(value switch
        {
            null => "NULL",
            bool b => b ? "1" : "0",
            string s => $"\"{s}\"",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        }).Append('\n');
}