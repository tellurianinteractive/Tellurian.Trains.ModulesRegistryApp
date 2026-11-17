using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace ModulesRegistry.Shared;

public class PageHistory : IDisposable
{
    private const int MaxHistorySize = 32;
    private readonly NavigationManager Navigator;
    private readonly List<VisitedPage> History;

    public PageHistory(NavigationManager navigationManager)
    {
        Navigator = navigationManager;
        History = new(MaxHistorySize)
        {
            new VisitedPage(Navigator.BaseUri)
        };
        Navigator.LocationChanged += OnLocationChanged;
    }

    public bool CanNavigateBack => History.Count > 1;

    public bool IsCurrentAlsoLast => History.Count != 0 && Navigator.Uri.Equals(History.Last().Url, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Goes to the previous page in this circuit, or to <paramref name="fallbackUrl"/> when there is none: a page reached
    /// from a static SSR page starts a new circuit with no history (Migration step 3.3). The fallback is usually a static
    /// page, so it is a full page load: the interactive router would otherwise handle the address itself, and an address
    /// that the static page only redirects from (such as /FreightCustomers to the user's country) shows "nothing to display".
    /// </summary>
    public void NavigateBack(string? fallbackUrl = null)
    {
        if (CanNavigateBack) Navigator.NavigateTo(History[^2].Url);
        else if (fallbackUrl is not null) Navigator.NavigateTo(fallbackUrl, forceLoad: true);
    }
    public bool IsShowningHelp
    {
        get => History.Count != 0 && History.Last().ShowHelp == true;
        set
        {
            if (History.Count != 0) History.Last().ShowHelp = value;
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        EnsureSize();

        if (IsCurrentAlsoLast) return;
        History.Add(new VisitedPage(e.Location));
    }

    private void EnsureSize()
    {
        if (History.Count <= MaxHistorySize) return;
        History.RemoveRange(0, History.Count - MaxHistorySize);
    }

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Navigator.LocationChanged -= OnLocationChanged;
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public record VisitedPage(string Url)
{
    public bool? ShowHelp { get; set; }
}
