using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq.Expressions;

namespace ModulesRegistry.Components
{
    public abstract class AppInputBase<TValue> : InputBase<TValue>
    {
        [Parameter] public string? Label { get; set; }
        [Parameter] public Expression<Func<string>>? ValidationFor { get; set; }
        [Parameter] public int? Width { get; set; }
        [Parameter] public bool IsDisabled { get; set; }

    }
}
