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

    public void NavigateBack()
    {
        //if ( IsCurrentAlsoLast)return;
        if (!CanNavigateBack) return;
        var page = History[^2];
        if (page == null) return;
        Navigator.NavigateTo(page.Url);
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
