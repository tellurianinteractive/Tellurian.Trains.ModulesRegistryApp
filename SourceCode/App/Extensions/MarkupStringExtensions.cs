using Microsoft.AspNetCore.Components;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Extensions;

/// <summary>
/// Helper methods for displaying things as markup
/// </summary>
public static class MarkupStringExtensions
{
    private static MarkupString AsMarkup(this string? markupText) =>
        string.IsNullOrWhiteSpace(markupText) ? new() : new(markupText);

    public static MarkupString StatusIcon(this Person? person) =>
        person.PersonStatusIcons().AsMarkup();

    private static string? PersonStatusIcons(this Person? person) =>
        person is null ? null : $"<span class=\"fa fa-{person.PersonStatusIconName()}\"/>";

    private static string PersonStatusIconName(this Person? person) =>
        person is null ? string.Empty :
        person.User?.IsGlobalAdministrator == true ? "user-shield" :
        person.User?.IsCountryAdministrator == true ? "user-gear" :
        person.PrimaryEmail().HasNoValue() ? "user-times" :
        person.IsInvited() && person.IsNeverLoggedIn() ? "envelope" :
        person.UserId.HasValue ? "user-check" :
        "user-slash";
}
