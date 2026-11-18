using ModulesRegistry.Resources;
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

    /// <summary>
    /// On a static page (an SSR form, Migration step 3.4) each field shows its own message, which is also where the
    /// browser's client-side validation puts it; interactive forms keep their <c>ValidationSummary</c>.
    /// </summary>
    protected bool ShowFieldValidationMessage => !RendererInfo.IsInteractive && ValueExpression is not null;

    public string WidthCss => Width.HasValue ? $"col-md-{Width}" : "col-md-4";

    public string LabelText(IStringLocalizer<Strings> localizer)
    {
        if (Label.HasNoValue()) return string.Empty;
        return localizer.FromParts(Label);
    }
}
