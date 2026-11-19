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

    public static MarkupString EmailIcon(this Person? person) =>
        person is null || person.EmailAddresses.IsEmpty ? new MarkupString() :
        new MarkupString($"""<a class="fa fa-envelope" href="mailto:{person.EmailAddresses}"></a>""");


    public static MarkupString ImageIcon(this Vehicle vehicle) =>
        (vehicle.HasImage() ? $"""<a target="_blank" href="{vehicle.PrototypeImageHref}"><span class="{FontAwesome.Image}"/></a>""" : "").AsMarkup();

    public static MarkupString ApprovedIcon(this MeetingParticipant participant) =>
        (participant.IsApproved() ? """<span class="fa fa-check-circle""/>""" : string.Empty).AsMarkup();
}
