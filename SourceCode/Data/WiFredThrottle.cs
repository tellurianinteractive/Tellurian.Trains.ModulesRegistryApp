using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

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
    public static bool IsMacAddressLocked(this WiFredThrottle it) => 
        it.ValidationDateTime.HasValue;

    public static string InventoryNumber(this WiFredThrottle it) =>
        $"{it.OwningPerson.FremoNumber()}-{it.InventoryNumber}";
        
    public static string OwnerDescription(this WiFredThrottle it) =>
        it.OwningPerson is null ? string.Empty :
        it.OwningPerson.Country is null ? $"{it.OwningPerson.Name()} {it.OwningPerson.CityName}" :
        string.Empty;


    /// <summary>
    /// Sets loco adresses to valid DCC-addresses; otherwise null;
    /// </summary>
    /// <param name="throttle"></param>
    public static void SetDccAddressOrNull(this WiFredThrottle throttle)
    {
        throttle.LocoAddress1 = throttle.LocoAddress1.DccAddressOrNull();
        throttle.LocoAddress2 = throttle.LocoAddress2.DccAddressOrNull();
        throttle.LocoAddress3 = throttle.LocoAddress3.DccAddressOrNull();
        throttle.LocoAddress4 = throttle.LocoAddress4.DccAddressOrNull();
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
