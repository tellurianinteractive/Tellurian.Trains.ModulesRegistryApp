namespace ModulesRegistry.Diagnostics;

/// <summary>
/// Logs as Critical the exceptions that escape everything else: in the process and in background tasks nobody awaits.
/// Requests and pages have their own handlers (<see cref="UnhandledRequestExceptionHandler"/>, <see cref="AppErrorBoundary"/>).
/// </summary>
public static class UnhandledExceptionLogging
{
    public const string LoggerCategory = "ModulesRegistry.Diagnostics.UnhandledExceptions";

    extension(WebApplication app)
    {
        public WebApplication LogUnhandledExceptions()
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(LoggerCategory);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => OnUnhandledException(logger, e);
            TaskScheduler.UnobservedTaskException += (_, e) => OnUnobservedTaskException(logger, e);
            return app;
        }
    }

    public static void OnUnhandledException(ILogger logger, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception exception) return;
        logger.UnhandledProcessException(exception, e.IsTerminating);
    }

    public static void OnUnobservedTaskException(ILogger logger, UnobservedTaskExceptionEventArgs e) =>
        logger.UnobservedTaskException(e.Exception);
}