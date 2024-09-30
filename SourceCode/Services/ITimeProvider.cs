namespace ModulesRegistry.Services
{
    public interface ITimeProvider
    {
        DateTimeOffset Now { get; }
        DateTime LocalTime { get; }
        DateOnly Today { get; }
    }

    public class SystemTimeProvider(TimeZoneInfo? timeZoneInfo = null) : ITimeProvider
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
        public DateOnly Today => DateOnly.FromDateTime(Now.Date);
        public DateTime LocalTime => TimeZoneInfo.ConvertTimeFromUtc(Now.UtcDateTime, TimeZoneInfo);

        private readonly TimeZoneInfo TimeZoneInfo = timeZoneInfo ?? TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
    }
}
