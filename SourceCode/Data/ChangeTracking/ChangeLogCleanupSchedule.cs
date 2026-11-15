namespace ModulesRegistry.Data.ChangeTracking;

/// <summary>
/// When the change log's age cleanup may run next: at most once a day per process. A singleton, shared by the scoped
/// <see cref="ChangeLogInterceptor"/>s. (The F1 plan sleeps when idle, so a timer would be unreliable.)
/// </summary>
public sealed class ChangeLogCleanupSchedule
{
    private long _nextTicks;

    /// <summary>
    /// True for one caller when the cleanup is due; the next one is then due a day later.
    /// </summary>
    public bool TryClaim(DateTimeOffset now)
    {
        var next = Interlocked.Read(ref _nextTicks);
        return now.UtcTicks >= next &&
            Interlocked.CompareExchange(ref _nextTicks, now.AddDays(1).UtcTicks, next) == next;
    }
}