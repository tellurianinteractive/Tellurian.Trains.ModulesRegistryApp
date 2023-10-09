using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace ModulesRegistry.Shared;

public class PageHistory
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

    public bool IsCurrentAlsoLast => History.Any() && Navigator.Uri.Equals(History.Last().Url, StringComparison.OrdinalIgnoreCase);

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
        get => History.Any() && History.Last().ShowHelp == true;
        set
        {
            if (History.Any()) History.Last().ShowHelp = value;
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

    public void Dispose()
    {
        Navigator.LocationChanged -= OnLocationChanged;
    }
}

public record VisitedPage(string Url)
{
    public bool? ShowHelp { get; set; }
}
