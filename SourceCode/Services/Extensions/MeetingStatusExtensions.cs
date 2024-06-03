﻿using Microsoft.AspNetCore.Components;

namespace ModulesRegistry.Services.Extensions;

public static class MeetingExtensions
{
    public static bool MayAdministerMeetings(this Meeting? meetingWithOrganiserGroupMembers, ClaimsPrincipal? principal) =>
        meetingWithOrganiserGroupMembers?.OrganiserGroup is null ? false :
        principal.IsCountryAdministratorInCountry(meetingWithOrganiserGroupMembers.OrganiserGroup.CountryId) ||
        meetingWithOrganiserGroupMembers.OrganiserGroup.GroupMembers.Any(gm => gm.IsAnyAdministrator());
}

public static class MeetingStatusExtensions
{
    public static string Status(this Meeting? meeting, DateTime at)
    {
        if (meeting is null) return string.Empty;
        var resourceKey = meeting.StatusResourceName(at);
        return Resources.Strings.ResourceManager.GetString(resourceKey) ?? resourceKey;
    }

    internal static string StatusResourceName(this Meeting? meeting, DateTime at) =>
        meeting is null ? string.Empty :
        meeting.IsCancelled() ? "Canceled" :
        meeting.IsOpenForRegistration(at) ? "RegistrationOpen" :
        meeting.IsClosedForRegistration(at) ? "RegistrationClosed" :
        meeting.IsOrganiserInternal ? "Internal" :
        ((MeetingStatus)meeting.Status).ToString();

    public static string StatusCssClass(this Meeting meeting, DateTime at) =>
        $"meeting {meeting.StatusResourceName(at).ToLowerInvariant()}";

    public static string StatusIcon(this Meeting meeting, DateTime at) =>
        meeting.StatusResourceName(at) switch
        {
            "Canceled" => "fa fa-times-circle",
            "RegistrationOpen" => "fa fa-user-plus",
            "RegistrationClosed" => "fa fa-user-slash",
            _ => (MeetingStatus)meeting.Status switch
            {
                MeetingStatus.Preliminary => "fa fa-question-circle",
                MeetingStatus.Planned => meeting.GroupDomainId.HasValue ? "fa fa-calendar-check" : "fa fa-check-circle",
                MeetingStatus.UnderApproval => "fa fa-flag",
                MeetingStatus.Approved  => meeting.GroupDomainId.HasValue ? "fa fa-check-double" : "fa fa-check-circle",

                _ => string.Empty,
            }
        };

    public static string StatusColor(this Meeting meeting, DateTime at) =>
        meeting.EndDate <= at ? "gray" :
        (MeetingStatus)meeting.Status switch
        {
            MeetingStatus.Canceled => "red",
            MeetingStatus.Approved => "green",
            MeetingStatus.Planned => meeting.GroupDomainId.HasValue ? "blue" : "green",
            MeetingStatus.Preliminary => "orange",
            _ => "blue"

        };

    public static MarkupString DisplayedStatus(this Meeting meeting, DateTime at) =>
        new(
            $"""
            <span class="{meeting.StatusIcon(at)}" style="color: {meeting.StatusColor(at)}" title="{meeting.Status(at)}"></span>
            """
            );


}

