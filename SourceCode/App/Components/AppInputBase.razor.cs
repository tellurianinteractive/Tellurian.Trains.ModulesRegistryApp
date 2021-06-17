using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;

namespace ModulesRegistry.Components
{
    public abstract partial class AppInputBase<TValue>
    {

        [Parameter] public string? Label { get; set; }
        [Parameter] public Expression<Func<string>>? ValidationFor { get; set; }
        [Parameter] public int? Width { get; set; }
        [Parameter] public bool IsDisabled { get; set; }
        [Parameter] public bool IsVisible { get; set; } = true;
        [Parameter] public RenderFragment? ChildContent { get; set; }

        public string WidthCss => Width.HasValue ? $"col-md-{Width}" : "col-md-4";
    }
}
