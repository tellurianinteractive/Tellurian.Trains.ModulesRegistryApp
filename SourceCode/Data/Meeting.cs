#nullable disable

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
    public string PlaceName { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today.AddYears(1);
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1).AddDays(4);
    public int Status { get; set; }
    public bool IsFremo { get; set; }
    public virtual Group OrganiserGroup { get; set; }
    public virtual ICollection<Layout> Layouts { get; set; }
    public virtual ICollection<MeetingParticipant> Participants { get; set; }

}

#nullable enable

public static class MeetingExtensions
{
    public static bool IsOpenForRegistration(this Meeting? it, DateTime at) =>
        it is not null && !it.IsCancelled() && it.Layouts.Any() && it.Layouts.Min(l => l.RegistrationOpeningDate) <= at && it.Layouts.Max(l => l.RegistrationClosingDate) > at;

    public static bool IsCancelled(this Meeting? it) => it is null || it.Status == (int)MeetingStatus.Canceled;
    public static int DaysCount(this Meeting it) => (it.EndDate - it.StartDate).Days + 1;
    public static string Day(this Meeting it, int day) => it.StartDate.AddDays(day - 1).DayOfWeek.ToString();
}
