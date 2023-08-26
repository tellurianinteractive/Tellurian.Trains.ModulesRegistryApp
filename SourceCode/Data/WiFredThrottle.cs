using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace ModulesRegistry.Data;

#nullable disable
public class WiFredThrottle
{
    public int Id { get; set; }
    public string MacAddress { get; set; }
    public int InventoryNumber { get; set; }
    public string Name { get; set; }
    public string Configuration { get; set; }
    public DateTimeOffset RegistrationDateTime { get; set; }
    public DateTimeOffset? ValidationDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }

    public short? LocoAddress1 { get; set; }
    public short? LocoAddress2 { get; set; }
    public short? LocoAddress3 { get; set; }
    public short? LocoAddress4 { get; set; }

    public int OwningPersonId { get; set; }
    public virtual Person OwningPerson { get; set; }
}

#nullable enable


public static class WiFredThrottleExtensions
{
    public static bool MayRegisterWiFred(this Person? person) =>
        person is not null && person.FremoMemberNumber.HasValue;

    public static bool IsMacAddressLocked(this WiFredThrottle it) => 
        it.ValidationDateTime.HasValue;

    public static string BarcodeId(this WiFredThrottle it) =>
        $"{it.OwningPerson.FremoNumber():0000000}{it.InventoryNumber:0000}";
        
    public static string OwnerDescription(this WiFredThrottle it) =>
        it.OwningPerson is null ? string.Empty :
        it.OwningPerson.Country is null ? $"{it.OwningPerson.Name()} {it.OwningPerson.CityName}" :
        string.Empty;

    public static string DccAddresses(this WiFredThrottle it)
    {
        var result = new List<short>(4);
        if (it.LocoAddress1.HasValue) result.Add(it.LocoAddress1.Value);
        if (it.LocoAddress2.HasValue) result.Add(it.LocoAddress2.Value);
        if (it.LocoAddress3.HasValue) result.Add(it.LocoAddress3.Value);
        if (it.LocoAddress4.HasValue) result.Add(it.LocoAddress4.Value);
        return string.Join(", ", result);
    }

    public static void SetMacAddressUppercase(this WiFredThrottle it)
    {
        it.MacAddress = it.MacAddress.ToUpperInvariant();
    }



    /// <summary>
    /// Sets loco adresses to valid DCC-addresses; otherwise null;
    /// </summary>
    /// <param name="it"></param>
    public static void SetDccAddressOrNull(this WiFredThrottle it)
    {
        it.LocoAddress1 = it.LocoAddress1.DccAddressOrNull();
        it.LocoAddress2 = it.LocoAddress2.DccAddressOrNull();
        it.LocoAddress3 = it.LocoAddress3.DccAddressOrNull();
        it.LocoAddress4 = it.LocoAddress4.DccAddressOrNull();
    }
}

public static class WiFredThrottleMapper
{
    public static void MapWiFredThrottle(this ModelBuilder builder) =>
        builder.Entity<WiFredThrottle> (entity =>
        {
            entity.ToTable("WiFredThrottle");
            entity.HasOne(p => p.OwningPerson)
                .WithMany()
                .HasForeignKey(w => w.OwningPersonId);
        });
}
