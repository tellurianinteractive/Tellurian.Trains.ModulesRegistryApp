using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Resources;

namespace ModulesRegistry.Components;

/// <summary>
/// The filter field under a column's title, for <see cref="FilterColumn{TGridItem, TProp}"/> and <see cref="FilterTemplateColumn{TGridItem}"/>.
/// It belongs to a GET form elsewhere on the page through the HTML <c>form</c> attribute, so the filter ends up in the query string.
/// </summary>
internal static class FilterField
{
    public static void Render(RenderTreeBuilder builder, string name, string form, string? value, string? title, IStringLocalizer<Strings> localizer)
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "type", "search");
        builder.AddAttribute(2, "name", name);
        builder.AddAttribute(3, "form", form);
        builder.AddAttribute(4, "value", value);
        builder.AddAttribute(5, "class", "form-control form-control-sm col-filter");
        builder.AddAttribute(6, "autocomplete", "off");
        builder.AddAttribute(7, "aria-label", title);
        // It isn't obvious that the fields are for filtering, so they say so (as placeholder and as tooltip).
        builder.AddAttribute(8, "placeholder", localizer["EnterSearchValue"].Value);
        builder.AddAttribute(9, "title", localizer["EnterSearchValue"].Value);
        builder.CloseElement();
    }
}