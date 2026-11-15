using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Diagnostics;

namespace ModulesRegistry.AppTests.Diagnostics;

/// <summary>
/// Unhandled exceptions are logged once each, as Critical (Migration step 2C.2): requests, pages, the process and background tasks.
/// </summary>
[TestClass]
public sealed class UnhandledExceptionTests
{
    private const string ThrowingPath = "/throw.test";

    [TestMethod]
    public async Task RequestHandlerLogsCriticalAndWritesTheErrorPage()
    {
        var logger = new FakeLogger<UnhandledRequestExceptionHandler>();
        var handler = new UnhandledRequestExceptionHandler(logger);
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/meetings";
        context.Request.QueryString = new QueryString("?apiKey=secret");
        context.Response.Body = new MemoryStream();
        var exception = new InvalidOperationException("boom");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.IsTrue(handled);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var record = logger.Collector.LatestRecord;
        Assert.AreEqual(1, logger.Collector.Count);
        Assert.AreEqual(LogLevel.Critical, record.Level);
        Assert.AreEqual(UnhandledExceptionLog.RequestEventId, record.Id.Id);
        Assert.AreSame(exception, record.Exception);
        Assert.AreEqual("Unhandled exception in request POST /api/meetings", record.Message);
        Assert.DoesNotContain("secret", record.Message, "The query string can hold an API key.");
    }

    [TestMethod]
    public async Task RequestExceptionInTheAppIsLoggedOnceAsCritical()
    {
        var logs = new FakeLogCollector();
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureLogging(logging => logging.AddProvider(new FakeLoggerProvider(logs)))
            .ConfigureServices(services => services.AddTransient<IStartupFilter, ThrowingEndpointStartupFilter>()));
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ThrowingPath);

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "An unexpected error has occurred");
        var withException = logs.GetSnapshot().Where(r => r.Exception is not null).ToList();
        Assert.HasCount(1, withException, string.Join(Environment.NewLine, withException.Select(r => $"{r.Level} {r.Category}: {r.Message}")));
        Assert.AreEqual(LogLevel.Critical, withException[0].Level);
        Assert.AreEqual(UnhandledExceptionLog.RequestEventId, withException[0].Id.Id);
    }

    [TestMethod]
    public async Task ErrorBoundaryLogsCriticalAndShowsTheErrorContent()
    {
        var logs = new FakeLogCollector();
        var services = new ServiceCollection()
            .AddLogging(logging => logging.AddProvider(new FakeLoggerProvider(logs)))
            .AddSingleton<NavigationManager>(new TestNavigationManager("https://localhost/layouts/5?participant=7"))
            .AddSingleton<IErrorBoundaryLogger, NoErrorBoundaryLogger>()
            .BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(AppErrorBoundary.ChildContent)] = (RenderFragment)(b => { b.OpenComponent<ThrowingComponent>(0); b.CloseComponent(); }),
                [nameof(AppErrorBoundary.ErrorContent)] = (RenderFragment<Exception>)(_ => b => b.AddContent(0, "Error shown")),
            });
            var output = await renderer.RenderComponentAsync<AppErrorBoundary>(parameters);
            return output.ToHtmlString();
        });

        Assert.AreEqual("Error shown", html);
        var record = logs.GetSnapshot().Single(r => r.Exception is not null);
        Assert.AreEqual(LogLevel.Critical, record.Level);
        Assert.AreEqual(UnhandledExceptionLog.ComponentEventId, record.Id.Id);
        Assert.AreEqual("Unhandled exception on page /layouts/5", record.Message);
    }

    [TestMethod]
    [DataRow("https://localhost/", "/")]
    [DataRow("https://localhost/Layouts/5", "/Layouts/5")]
    [DataRow("https://localhost/Layouts/5?id=1#top", "/Layouts/5")]
    [DataRow("https://localhost/Layouts#top", "/Layouts")]
    public void PagePathHasNoQueryString(string uri, string expected) =>
        Assert.AreEqual(expected, AppErrorBoundary.PagePath(new TestNavigationManager(uri)));

    [TestMethod]
    public void ProcessExceptionIsLoggedAsCritical()
    {
        var logger = new FakeLogger();
        var exception = new InvalidOperationException("boom");

        UnhandledExceptionLogging.OnUnhandledException(logger, new UnhandledExceptionEventArgs(exception, isTerminating: true));

        Assert.AreEqual(LogLevel.Critical, logger.LatestRecord.Level);
        Assert.AreEqual(UnhandledExceptionLog.ProcessEventId, logger.LatestRecord.Id.Id);
        Assert.AreSame(exception, logger.LatestRecord.Exception);
    }

    [TestMethod]
    public void ProcessExceptionThatIsNotAnExceptionObjectIsIgnored()
    {
        var logger = new FakeLogger();

        UnhandledExceptionLogging.OnUnhandledException(logger, new UnhandledExceptionEventArgs("not an exception", isTerminating: false));

        Assert.AreEqual(0, logger.Collector.Count);
    }

    [TestMethod]
    public void UnobservedTaskExceptionIsLoggedAsCritical()
    {
        var logger = new FakeLogger();
        var exception = new AggregateException(new InvalidOperationException("boom"));

        UnhandledExceptionLogging.OnUnobservedTaskException(logger, new UnobservedTaskExceptionEventArgs(exception));

        Assert.AreEqual(LogLevel.Critical, logger.LatestRecord.Level);
        Assert.AreEqual(UnhandledExceptionLog.UnobservedTaskEventId, logger.LatestRecord.Id.Id);
        Assert.AreSame(exception, logger.LatestRecord.Exception);
    }

    /// <summary>Adds an endpoint after the app's own pipeline that always throws.</summary>
    private sealed class ThrowingEndpointStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            app.Use((HttpContext context, RequestDelegate _) =>
                context.Request.Path == ThrowingPath ? throw new InvalidOperationException("Test exception") : Task.CompletedTask);
        };
    }

    private sealed class ThrowingComponent : ComponentBase
    {
        protected override void OnInitialized() => throw new InvalidOperationException("Test exception");
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string uri) => Initialize("https://localhost/", uri);
    }

    private sealed class NoErrorBoundaryLogger : IErrorBoundaryLogger
    {
        public ValueTask LogErrorAsync(Exception exception) => ValueTask.CompletedTask;
    }
}