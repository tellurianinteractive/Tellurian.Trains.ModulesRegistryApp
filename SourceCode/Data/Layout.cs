using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

#nullable disable

public class Layout
{
    public Layout()
    {
        LayoutParticipants = new HashSet<LayoutParticipant>();
    }
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public int OrganisingGroupId { get; set; }
    public int? ContactPersonId { get; set; }
    public int PrimaryModuleStandardId { get; set; }
    public bool IsRegistrationPermitted { get; set; } = true;
    public DateTime RegistrationOpeningDate { get; set; }
    public DateTime RegistrationClosingDate { get; set; }
    public DateTime? ModuleRegistrationClosingDate { get; set; }
    public string ShortName { get; set; }
    public string Theme { get; set; }
    public string Details { get; set; }
    public short? FirstYear { get; set; }
    public short? LastYear { get; set; }

    public virtual Meeting Meeting { get; set; }
    public virtual Group OrganisingGroup { get; set; }
    public virtual Person ContactPerson { get; set; }
    public virtual ModuleStandard PrimaryModuleStandard { get; set; }
    public virtual ICollection<LayoutParticipant> LayoutParticipants { get; set; }
    public override string ToString() => $"{Meeting?.Name} {PrimaryModuleStandard?.ShortName}";
}

# nullable enable

public static class LayoutExtensions
{

    public static string RegistrationOpensDate(this Layout layout) => layout.RegistrationOpeningDate.ToShortDateString();
    public static string RegistrationClosesDate(this Layout layout) => layout.RegistrationClosingDate.ToShortDateString();
    public static string RegistrationOfModulesClosesDate(this Layout layout) => (layout.ModuleRegistrationClosingDate ?? layout.RegistrationClosingDate).ToShortDateString();
    public static DateTime ModuleRegistrationClosingDate(this Layout layout) => layout.ModuleRegistrationClosingDate ?? layout.RegistrationClosingDate;

    public static string Description(this Layout? me) =>
        me is null || me.PrimaryModuleStandard is null ? string.Empty :
        me.PrimaryModuleStandard.Scale is null ? me.PrimaryModuleStandard.ShortName :
        me.PrimaryModuleStandard.Scale.ShortName;

    public static string DescriptionWithName(this Layout? me) =>
        me is null ? string.Empty :
        $"{me.Description()} {me.ShortName}";
 

    public static string DescriptionWithMeetingAndLayoutName(this Layout? me) =>
        me is null ? string.Empty :
        me.Meeting is null ? $"{me.Description()} {me.ShortName}" :
        $"{me.Meeting.Name}, {me.Description()} {me.ShortName}";

    public static string DescriptionWithTheme(this Layout? me) =>
        me is null ? string.Empty :
        me.Theme.HasValue() ? $"{me.Meeting.Name} {me.Description()}-{me.Theme}" :
        me.Meeting is not null ? $"{me.Meeting.Name} {me.Description()} - {me.Theme}" :
        $"{me.Description} {me.Theme}";

    internal static bool IsOpenForRegistration(this Layout layout, DateTime at) =>
       layout.IsRegistrationPermitted &&
       layout.RegistrationOpeningDate <= at &&
       layout.RegistrationClosingDate.AddDays(1) >= at;

    internal static bool IsNotYetOpenForRegistration(this Layout layout, DateTime at) =>
        layout.IsRegistrationPermitted &&
        layout.RegistrationOpeningDate > at;


}

public static class LayoutMapping
{
    public static void MapLayout(this ModelBuilder builder) =>
        builder.Entity<Layout>(entity =>
        {
            entity.Property(e => e.Details)
                .HasMaxLength(50);

            entity.ToTable("Layout",
                tb => tb.HasTrigger("DeleteLayout"));

            entity.HasOne(d => d.Meeting)
                .WithMany(p => p.Layouts)
                .HasForeignKey(d => d.MeetingId);

            entity.HasOne(d => d.OrganisingGroup)
                .WithMany()
                .HasForeignKey(d => d.OrganisingGroupId);

            entity.HasOne(d => d.ContactPerson)
                .WithMany()
                .HasForeignKey(d => d.ContactPersonId);

            entity.HasOne(d => d.PrimaryModuleStandard)
                .WithMany()
                .HasForeignKey(d => d.PrimaryModuleStandardId);
            entity.HasMany(d => d.LayoutParticipants)
                .WithOne()
                .HasForeignKey(d => d.LayoutId);
        });
}
