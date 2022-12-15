#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

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
    public int? StartWeekdayId { get; set; }
    public string Theme { get; set; }
    public string Details { get; set; }
    public short? FirstYear { get; set; }
    public short? LastYear { get; set; }
    public short? StartHour { get; set; }
    public short? EndHour { get; set; }

    public virtual Meeting Meeting { get; set; }
    public virtual Group OrganisingGroup { get; set; }
    public virtual Person ContactPerson { get; set; }
    public virtual ModuleStandard PrimaryModuleStandard { get; set; }
    public virtual OperatingDay StartWeekday { get; set; }

    public virtual ICollection<LayoutParticipant> LayoutParticipants { get; set; }

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
        });
}
