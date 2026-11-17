using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace ModulesRegistry.Components;

/// <summary>
/// A <see cref="PropertyColumn{TGridItem, TProp}"/> with a filter field under its title (Migration step 3.3). QuickGrid's
/// column options pop-up needs a circuit, also in .NET 11, so the field is always shown in the header instead. It belongs to
/// a GET form elsewhere on the page through the HTML <c>form</c> attribute, so the filter ends up in the query string;
/// with <c>data-auto-submit</c> on that form, wwwroot/js/filter-form.js filters as the user types.
/// </summary>
public class FilterColumn<TGridItem, TProp> : PropertyColumn<TGridItem, TProp>
{
    /// <summary>The query string parameter of the filter.</summary>
    [Parameter, EditorRequired] public string FilterName { get; set; } = string.Empty;
    /// <summary>The current filter value, usually from the query string.</summary>
    [Parameter] public string? FilterValue { get; set; }
    /// <summary>The id of the filter form.</summary>
    [Parameter, EditorRequired] public string FilterForm { get; set; } = string.Empty;

    public FilterColumn()
    {
        // Keeps QuickGrid's own header (the title, a sort link when sortable) and adds the field below it.
        var title = HeaderContent;
        HeaderContent = builder =>
        {
            title(builder);
            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "type", "search");
            builder.AddAttribute(2, "name", FilterName);
            builder.AddAttribute(3, "form", FilterForm);
            builder.AddAttribute(4, "value", FilterValue);
            builder.AddAttribute(5, "class", "form-control form-control-sm col-filter");
            builder.AddAttribute(6, "autocomplete", "off");
            builder.AddAttribute(7, "aria-label", Title);
            builder.CloseElement();
        };
    }
}