using Microsoft.AspNetCore.Components;
using ModulesRegistry.Diagnostics;

namespace ModulesRegistry.Components
{
    public partial class ActionMessage
    {
        [Parameter]
        public string Label { get; set; } = "ActionInProgress";
        [Parameter]
        public string AlertType { get; set; } = "info";
        [CascadingParameter] private HttpContext? HttpContext { get; set; }
        [Inject] private ILogger<ActionMessage> Logger { get; set; } = default!;

        // A failure shown to the user is logged (event 4001), once per request: static SSR renders the page once.
        protected override void OnParametersSet()
        {
            if (IsFailure) Logger.FailureShown(HttpContext, Label);
        }

        bool IsFailure => AlertType.ToLowerInvariant() is "stop" or "error";
        string AlertCss => $"alert alert-{AlertBootstrapName} mt-3";
        string IconCss => AlertType.ToLowerInvariant() switch
        {
            "error" => "fa fa-bug",
            "stop" => "fa fa-exclamation-circle",
            "wait" => "fa fa-hourglass",
            "warning" => "fa fa-exclamation-triangle",
            "spinning" => "fa fa-spinner",
            _ => "fa fa-info-circle",
        };

        string AlertBootstrapName => AlertType.ToLowerInvariant() switch
        {
            "wait" => "primary",
            "warning" => "warning",
            "error" => "danger",
            "stop" => "danger",
            _ => "info",
        };
    }
}