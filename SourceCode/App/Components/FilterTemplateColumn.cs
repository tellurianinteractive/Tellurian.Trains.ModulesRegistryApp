using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Resources;

namespace ModulesRegistry.Components;

/// <summary>
/// A <see cref="TemplateColumn{TGridItem}"/> with a filter field under its title, as <see cref="FilterColumn{TGridItem, TProp}"/>,
/// for a column whose cells show more than a property (for example a symbol, or a colour). Give it <c>SortBy</c> to make it sortable.
/// </summary>
public class FilterTemplateColumn<TGridItem> : TemplateColumn<TGridItem>
{
    /// <summary>The query string parameter of the filter.</summary>
    [Parameter, EditorRequired] public string FilterName { get; set; } = string.Empty;
    /// <summary>The current filter value, usually from the query string.</summary>
    [Parameter] public string? FilterValue { get; set; }
    /// <summary>The id of the filter form.</summary>
    [Parameter, EditorRequired] public string FilterForm { get; set; } = string.Empty;
    [Inject] private IStringLocalizer<Strings> Localizer { get; set; } = default!;

    public FilterTemplateColumn()
    {
        // Keeps QuickGrid's own header (the title, a sort link when sortable) and adds the field below it.
        var title = HeaderContent;
        HeaderContent = builder =>
        {
            title(builder);
            FilterField.Render(builder, FilterName, FilterForm, FilterValue, Title, Localizer);
        };
    }
}