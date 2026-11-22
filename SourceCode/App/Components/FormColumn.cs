namespace ModulesRegistry.Components;

/// <summary>
/// The Bootstrap column classes for a form field of a width in twelfths (2026-11-22). The width applies from the xl breakpoint;
/// from md it is doubled (at most 12), since a field of width 1 or 2 is too narrow for its label and value on a tablet. A full row
/// then becomes two rows; AppFormRow starts each line of fields. Below md the fields stack.
/// </summary>
public static class FormColumn
{
    public const int DefaultWidth = 4;

    public static string Css(int? width)
    {
        var columns = Math.Clamp(width ?? DefaultWidth, 1, 12);
        return $"col-md-{Math.Min(2 * columns, 12)} col-xl-{columns}";
    }
}