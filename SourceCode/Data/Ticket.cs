namespace ModulesRegistry.Data;
public record Ticket(LayoutStation From, LayoutStation To, LayoutStation Seller)
{
    public const int ItemsPerPage = 12;
    public Country? Country => Seller.Country();
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
                    yield return new Ticket(fromStation, toStation, fromStation);
                    yield return new Ticket(toStation, fromStation, fromStation);
                }
            }
        }
    }

    
}
