#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public class Meeting
{
    public Meeting()
    {
        Layouts = new HashSet<Layout>();
        Participants = new HashSet<MeetingParticipant>();
    }

    public int Id { get; set; }
    public int OrganiserGroupId { get; set; }
    public bool IsOrganiserInternal { get; set; }
    public int? GroupDomainId { get; set; }
    public string CityName { get; set; }
    public string VenueName { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today.AddYears(1);
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1).AddDays(4);
    public int Status { get; set; }
    public string Details { get; set; }
    public string Accomodation { get; set; }
    public string Food { get; set; }
    public virtual Group OrganiserGroup { get; set; }
    public virtual GroupDomain GroupDomain { get; set; }
    public virtual ICollection<Layout> Layouts { get; set; }
    public virtual ICollection<MeetingParticipant> Participants { get; set; }
}

#nullable enable

public static class MeetingExtensions
{
    internal static void MapMeeting(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("Meeting", tb => tb.HasTrigger("DeleteMeeting"));

            entity.HasOne(d => d.OrganiserGroup)
                .WithMany()
                .HasForeignKey(d => d.OrganiserGroupId);

            entity.HasOne(d => d.GroupDomain)
                .WithMany()
                .HasForeignKey(d => d.GroupDomainId);

            entity.HasMany(d => d.Participants)
                .WithOne(d => d.Meeting)
                .HasForeignKey(d => d.MeetingId);
        });
}

