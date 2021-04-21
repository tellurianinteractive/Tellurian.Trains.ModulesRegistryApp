using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Collections.Generic;
using System.Linq;

namespace ModulesRegistry.Shared
{
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
                new VisitedPage(Navigator.Uri)
            };
            Navigator.LocationChanged += OnLocationChanged;
        }

        public void NavigateTo(string url)
        {
            Navigator.NavigateTo(url);
        }

        public bool CanNavigateBack => History.Count >= 2;

        public void NavigateBack()
        {
            if (!CanNavigateBack) return;
            var page = History[^2];
            History.RemoveRange(History.Count - 2, 2);
            Navigator.NavigateTo(page.Url);
        }

        public bool? IsShowningHelp
        {
            get => History.Last().ShowHelp;
            set => History.Last().ShowHelp = value;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            EnsureSize();
            History.Add(new VisitedPage(e.Location));
        }

        private void EnsureSize()
        {
            if (History.Count < MaxHistorySize) return;
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
}
