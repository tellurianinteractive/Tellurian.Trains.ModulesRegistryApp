#nullable disable

using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ModulesRegistry.Data;

public class LayoutParticipant
{
    public LayoutParticipant()
    {
        LayoutModules = new HashSet<LayoutModule>();
        LayoutStations = new HashSet<LayoutStation>();
    }
    public int Id { get; set; }
    public int MeetingParticipantId { get; set; }
    public int LayoutId { get; set; }
    public int PersonId { get; set; }
    public virtual MeetingParticipant MeetingParticipant { get; set; }
    public virtual Layout Layout { get; set; }
    public virtual Person Person { get; set; }

    public virtual ICollection<LayoutModule> LayoutModules { get; set; }
    public virtual ICollection<LayoutStation> LayoutStations { get; set; }
    public override string ToString() => $"{Person?.Name()}";

}

public static class LayoutParticipantExtensions
{
    public static bool IsValid(this LayoutParticipant me) =>
        me is not null && me.MeetingParticipantId > 0 && me.LayoutId > 0 && me.PersonId > 0;

    public static IEnumerable<LayoutModule> AllLayoutModules(this LayoutParticipant layoutParticipant, IEnumerable<LayoutModule> layoutModules)
    {
        if (layoutParticipant.LayoutModules.Count > 0 && layoutParticipant.LayoutModules.All(lm => lm.Module.ModuleOwnerships.All(mo =>  mo.OwnedShare == 1))) 
            return layoutParticipant.LayoutModules;
        return layoutModules.Where(lm => lm.Module.ModuleOwnerships.Any(mo => mo.PersonId == layoutParticipant.PersonId));
    }

}

internal static class LayoutParticipantMapping
{
    public static void MapLayoutParticipant(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LayoutParticipant>(entity =>
        {
            entity.ToTable("LayoutParticipant",
                tb => tb.HasTrigger("DeleteLayoutParticipant"));

            entity.HasOne(e => e.Person)
                .WithMany()
                .HasForeignKey(e => e.PersonId);

            entity.HasOne(e => e.MeetingParticipant)
                .WithMany(e => e.LayoutParticipations)
                .HasForeignKey(e => e.MeetingParticipantId);

            entity.HasOne(e => e.Layout)
                .WithMany(e => e.LayoutParticipants)
                .HasForeignKey(e => e.LayoutId);

            entity.HasMany(e => e.LayoutModules)
                .WithOne(e => e.LayoutParticipant)
                .HasForeignKey(e => e.LayoutParticipantId);

            entity.HasMany(e => e.LayoutStations)
                .WithOne(e => e.LayoutParticipant)
                .HasForeignKey(e => e.LayoutParticipantId);
        });
}

