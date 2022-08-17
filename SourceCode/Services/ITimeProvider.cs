namespace ModulesRegistry.Services
{
    public interface ITimeProvider
    {
        DateTimeOffset Now { get; }
        DateTime LocalTime { get; }
    }

    public class SystemTimeProvider : ITimeProvider
    {
        public SystemTimeProvider(TimeZoneInfo? timeZoneInfo = null)
        {
            TimeZoneInfo = timeZoneInfo ?? TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
        public DateTimeOffset Now => DateTimeOffset.Now;
        public DateTime LocalTime => TimeZoneInfo.ConvertTimeFromUtc(Now.UtcDateTime, TimeZoneInfo);

        private readonly TimeZoneInfo TimeZoneInfo;
    }
}
