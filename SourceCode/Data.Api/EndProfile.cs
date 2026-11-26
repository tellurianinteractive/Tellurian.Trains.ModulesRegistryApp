namespace ModulesRegistry.Data.Api;

/// <summary>An end profile drawing saved in the Registry, with the end plate's measures from the drawing, in whole millimetres.</summary>
public record EndProfileDrawingSaved(int EndProfileId, int? WidthMillimeter, int? HeightMillimeter);