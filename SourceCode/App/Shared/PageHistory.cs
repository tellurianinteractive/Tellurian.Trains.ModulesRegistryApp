using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Collections.Generic;

namespace ModulesRegistry.Shared
{
    public class PageHistory
    {
        private const int MaxHistorySize = 32;
        private readonly NavigationManager Navigator;
        private readonly List<string> History;

        public PageHistory(NavigationManager navigationManager)
        {
            Navigator = navigationManager;
            History = new(MaxHistorySize)
            {
                Navigator.Uri
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
            var backPageUrl = History[^2];
            History.RemoveRange(History.Count - 2, 2);
            Navigator.NavigateTo(backPageUrl);
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            EnsureSize();
            History.Add(e.Location);
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
}
