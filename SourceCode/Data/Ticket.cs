namespace ModulesRegistry.Data;
public record Ticket(LayoutStation From, LayoutStation To)
{
    public const int ItemsPerPage = 12;

}

public static class TicketBuilder
{
    public static IEnumerable<Ticket> CreateTickets(this IEnumerable<LayoutStation> layoutStations)
    {
        foreach (var fromStation in layoutStations)
        {
            foreach (var toStation in layoutStations)
            {
                if (fromStation.Id != toStation.Id)
                {
                    yield return new Ticket(fromStation, toStation);
                    yield return new Ticket(toStation, fromStation);
                }
            }
        }
    }

    public static string Validity(this Ticket ticket) =>
        string.Format(Resources.Strings.TicketIsValidTo, ticket.From.LayoutParticipant.MeetingParticipant.Meeting.EndDateOrTimes());
}
