namespace ModulesRegistry.Services
{
    public interface ITimeProvider
    {
        DateTimeOffset Now { get; }
    }

    public class SystemTimeProvider : ITimeProvider
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
