using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;
public class MeetingAdministrator
{
    public int PersonId { get; set; }
    public int MeetingId { get; set; }
    public int GroupId { get; set; }
}

internal static class MeetingAdministratorMapper
{
    public static void MapMeetingAdministrator(this ModelBuilder builder) =>
        builder.Entity<MeetingAdministrator>(entity =>
        {
            entity.ToView("MeetingAdministrator").HasNoKey();
        });
}

