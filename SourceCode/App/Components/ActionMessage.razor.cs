using Microsoft.AspNetCore.Components;

namespace ModulesRegistry.Components
{
    public partial class ActionMessage
    {
        [Parameter]
        public string Label { get; set; } = "ActionInProgress";
        [Parameter]
        public string AlertType { get; set; } = "info";
        string AlertCss => $"alert alert-{AlertBootstrapName} mt-3";
        string IconCss => AlertType.ToLowerInvariant() switch
        {
            "error" => "fa fa-bug",
            "stop" => "fa fa-exclamation-circle",
            "wait" => "fa fa-hourglass",
            "warning" => "fa fa-exclamation-triangle",
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