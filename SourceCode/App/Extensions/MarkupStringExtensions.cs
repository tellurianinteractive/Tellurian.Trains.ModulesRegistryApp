using Microsoft.AspNetCore.Components;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModulesRegistry.Extensions
{
    /// <summary>
    /// Helper methods for displaying things as markup
    /// </summary>
    public static class MarkupStringExtensions
    {
        private static MarkupString AsMarkup(this string? markupText) =>
            string.IsNullOrWhiteSpace(markupText) ? new() : new (markupText);

        public static MarkupString StatusIcon(this Person? me) =>
            me.PersonStatusIcons().AsMarkup();

        private static string? PersonStatusIcons(this Person? me) =>
            me is null ? null : $"<span class=\"fa fa-{me.PersonStatusIconName()}\"/>";

        private static string PersonStatusIconName(this Person? me) =>
            me is null ? string.Empty :
            me.PrimaryEmail().HasNoValue() ? "user-times" :
            me.IsInvited() && me.IsNeverLoggedIn() ? "envelope" :
            me.UserId.HasValue ? "user-check" :
            "user-slash";

    }
}
