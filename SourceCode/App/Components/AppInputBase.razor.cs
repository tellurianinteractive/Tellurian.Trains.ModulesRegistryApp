using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Extensions;
using System.Linq.Expressions;

namespace ModulesRegistry.Components;

public abstract partial class AppInputBase<TValue>
{

    [Parameter] public string? Label { get; set; }
    [Parameter] public Expression<Func<string>>? ValidationFor { get; set; }
    [Parameter] public int? Width { get; set; }
    [Parameter] public bool IsDisabled { get; set; }
    [Parameter] public bool IsVisible { get; set; } = true;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public Action? OnChangeNotifier { get; set; }

    public void OnChange() => OnChangeNotifier?.Invoke();

    public string WidthCss => Width.HasValue ? $"col-md-{Width}" : "col-md-4";

    public string LabelText(IStringLocalizer<App> localizer)
    {
        if (Label.HasNoValue()) return string.Empty;
        return localizer.FromParts(Label);
    }
}
