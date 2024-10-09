#nullable disable

using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static ModulesRegistry.Data.Resources.Strings;

namespace ModulesRegistry.Data;
#pragma warning disable IDE0028 // Simplify collection initialization

public partial class Module
{
    public Module()
    {
        ModuleExits = new HashSet<ModuleExit>();
        ModuleOwnerships = new HashSet<ModuleOwnership>();
        IsStandAlone = true;
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public int ScaleId { get; set; }
    public int StandardId { get; set; }
    public string Theme { get; set; }
    public short? RepresentsFromYear { get; set; }
    public short? RepresentsUptoYear { get; set; }
    public double? Radius { get; set; }
    public double? Angle { get; set; }
    public double? Straight { get; set; }
    public double Length { get; set; }
    public double? Width { get; set; }
    public short NumberOfThroughTracks { get; set; }
    public bool HasNormalGauge { get; set; }
    public bool HasNarrowGauge { get; set; }
    public bool Is2R { get; set; }
    public bool Is3R { get; set; }
    public bool IsTurntable { get; set; }
    public bool IsDuckunder { get; set; }
    public bool IsStandAlone { get; set; }
    public bool IsUnavailable { get; set; }
    public bool HasIntegratedLocoNet { get; set; }
    public int ObjectVisibilityId { get; set; }
    public int FunctionalState { get; set; }
    public int LandscapeState { get; set; }
    public int? DwgDrawingId { get; set; }
    public int? SkpDrawingId { get; set; }
    public int? PdfDocumentationId { get; set; }
    public string Note { get; set; }
    public int? StationId { get; set; }
    public int? FremoNumber { get; set; }
    public string PackageLabel { get; set; }
    public string ConfigurationLabel { get; set; }
    public short? NumberOfSections { get; set; }
    public int SignalFeature { get; set; }
    public int OverheadLineFeature { get; set; }
    public short? SpeedLimit { get; set; }
    public int LandscapeSeason { get; set; }

    public virtual Document DwgDrawing { get; set; }
    public virtual Document SkpDrawing { get; set; }
    public virtual Document PdfDocumentation { get; set; }
    public virtual Scale Scale { get; set; }
    public virtual ModuleStandard Standard { get; set; }
    public virtual Station Station { get; set; }
    public virtual ICollection<ModuleExit> ModuleExits { get; set; }
    public virtual ICollection<ModuleOwnership> ModuleOwnerships { get; set; }
}

# nullable enable
public static class ModuleExtensions
{
    public static MarkupString Info(this Module me)
    {
        var text = new StringBuilder(100);
        text.Append(me.Standard is null ? "" : $"{me.Standard.ShortName}: ");
        if (me.IsCurve())
        {
            text.Append(Curve);
            text.Append(' ');
            text.Append(me.NumberOfThroughTracks == 0 ? "" : me.NumberOfThroughTracks == 1 ? SingleTrack : me.NumberOfThroughTracks == 2 ? DoubleTrack : $"{me.NumberOfThroughTracks} {Tracks.ToLowerInvariant()}");
            text.Append($" {me.Angle!.Value}&deg;");
            text.Append($" r={me.Radius!.Value}mm");
        }
        else if (me.IsStraight())
        {
            text.Append(Straight);
            text.Append(' ');
            text.Append(me.NumberOfThroughTracks == 0 ? "" : me.NumberOfThroughTracks == 1 ? SingleTrack : me.NumberOfThroughTracks == 2 ? DoubleTrack : $"{me.NumberOfThroughTracks} {Tracks.ToLowerInvariant()}");
            text.Append($" {me.Length}mm");
        }
        else if (me.IsOperationsPlace())
        {
            var tracksCount = me.Station.StationTracks.Count;
            text.Append(OperationsPlace);
            text.Append(' ');
            text.Append(tracksCount == 0 ? "" : $"{tracksCount} {Tracks.ToLowerInvariant()}");
            text.Append($" {me.Length}mm");
            if (me.Station.IsShadow) text.Append($", {ShadowStation}");
            if (me.Station.IsJunction) text.Append($", {Junction}");
        }
        if (me.Is2R && me.Is3R)
        {
            text.Append(", 2R/3R");
        }
        else if (me.Is3R)
        {
            text.Append(", 3R");
        }
        if (me.Note.HasValue())
        {
            text.Append(", ");
            text.Append(me.Note);
        }
        return new(text.ToString());
    }

    private static bool IsCurve(this Module me) => !me.IsOperationsPlace() && me.Angle.HasValue;
    private static bool IsStraight(this Module me) => !me.IsOperationsPlace() && !me.Angle.HasValue;
    private static bool IsOperationsPlace(this Module me) => me.Station is not null;

    public static Module Clone(this Module me) =>
        new()
        {
            Angle = me.Angle,
            ConfigurationLabel = me.ConfigurationLabel,
            FullName = me.CloneFullName(),
            FunctionalState = me.FunctionalState,
            HasIntegratedLocoNet = me.HasIntegratedLocoNet,
            HasNarrowGauge = me.HasNarrowGauge,
            HasNormalGauge = me.HasNormalGauge,
            Is2R = me.Is2R,
            Is3R = me.Is3R,
            IsDuckunder = me.IsDuckunder,
            IsStandAlone = me.IsStandAlone,
            IsTurntable = me.IsTurntable,
            IsUnavailable = me.IsUnavailable,
            LandscapeSeason = me.LandscapeSeason,
            LandscapeState = me.LandscapeState,
            Length = me.Length,
            ModuleExits = me.ModuleExits.Select(it => new ModuleExit
            {
                Direction = it.Direction,
                EndProfileId = it.EndProfileId,
                Label = it.Label
            }).ToArray(),
            ModuleOwnerships = me.ModuleOwnerships.Select(it => new ModuleOwnership
            {
                PersonId = it.PersonId,
                GroupId = it.GroupId,
                OwnedShare = it.OwnedShare
            }).ToArray(),
            Note = me.Note,
            NumberOfSections = me.NumberOfSections,
            NumberOfThroughTracks = me.NumberOfThroughTracks,
            ObjectVisibilityId = me.ObjectVisibilityId,
            OverheadLineFeature = me.OverheadLineFeature,
            PackageLabel = me.PackageLabel,
            Radius = me.Radius,
            RepresentsFromYear = me.RepresentsFromYear,
            RepresentsUptoYear = me.RepresentsUptoYear,
            ScaleId = me.ScaleId,
            SignalFeature = me.SignalFeature,
            SpeedLimit = me.SpeedLimit,
            StandardId = me.StandardId,
            Straight = me.Straight,
            Theme = me.Theme,
            Width = me.Width,
        };

    public static string PackageName(this Module module) => module.PackageLabel.HasValue() ? module.PackageLabel : module.FullName;
    public static ModuleOwnership? ModuleOwnership(this Module module, ModuleOwnershipRef ownershipRef) =>
        module.ModuleOwnerships.SingleOrDefault(mo => mo.GroupId == ownershipRef.GroupId || mo.PersonId == ownershipRef.PersonId);

    static string CloneFullName(this Module module)
    {
        var random = new Random();
        var appended = $"-{random.Next(1, 1000)}";
        var totalLength = module.FullName.Length + appended.Length;
        if (totalLength <= 50) return $"{module.FullName}{appended}";
        return $"{module.FullName[..(50 - appended.Length)]}{appended}";
    }

    public static bool IsPartOfStation([NotNullWhen(true)] this Module me) => me.StationId.HasValue;

    public static IEnumerable<int> DocumentIds(this Module me)
    {
        if (me.PdfDocumentationId.HasValue) yield return me.PdfDocumentationId.Value;
        if (me.DwgDrawingId.HasValue) yield return me.DwgDrawingId.Value;
        if (me.SkpDrawingId.HasValue) yield return me.SkpDrawingId.Value;
    }

    public static double CalculateLength(this Module me)
    {
        double? curveLength = me.Angle.HasValue && me.Radius.HasValue ? Math.Round(Math.PI * me.Angle.Value * me.Radius.Value / 180.0, 0) : null;
        if (curveLength.HasValue && me.Straight.HasValue) return curveLength.Value + me.Straight.Value;
        if (curveLength.HasValue) return curveLength.Value;
        if (me.Straight.HasValue) return me.Straight.Value;
        return 0.0;
    }

}

public static class ModuleExtensionsForFremoName
{
    public static string? FremoName(this Module me)
    {
        if (me.FremoNumber.HasValue && me.ModuleOwnerships?.Count > 0)
        {
            if (me.ModuleOwnerships.First().Person?.FremoOwnerSignature is not null)
            {
                return $"{me.ModuleOwnerships.First().Person.FremoOwnerSignature}{me.FremoNumber.Value:000}";
            }
            else if (me.ModuleOwnerships.First().Group is not null)
            {
                return $"{me.ModuleOwnerships.First().Group.ShortName}{me.FremoNumber.Value:000}";
            }
        }
        return null;
    }

    public static bool HasAnyFremoName(this IEnumerable<Module>? modules) =>
        modules?.Any(m => m.FremoNumber.HasValue && m.ModuleOwnerships.Any(mo => (mo.Person?.FremoOwnerSignature.HasValue() == true) || (mo.Group?.ShortName.HasValue() == true))) == true;
}

public static class ModuleMapping
{
    internal static void MapModule(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("Module");

            entity.Property(e => e.ConfigurationLabel).HasMaxLength(10);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Note).HasMaxLength(255);

            entity.Property(e => e.NumberOfThroughTracks).HasDefaultValueSql("((1))");

            entity.Property(e => e.PackageLabel).HasMaxLength(10);

            entity.Property(e => e.Theme).HasMaxLength(50);

            entity.HasOne(d => d.DwgDrawing)
                .WithMany()
                .HasForeignKey(d => d.DwgDrawingId);

            entity.HasOne(d => d.SkpDrawing)
                .WithMany()
                .HasForeignKey(d => d.SkpDrawingId);

            entity.HasOne(d => d.PdfDocumentation)
                .WithMany()
                .HasForeignKey(d => d.PdfDocumentationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Scale)
                .WithMany(p => p.Modules)
                .HasForeignKey(d => d.ScaleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Station)
                  .WithMany(p => p.Modules)
                  .HasForeignKey(d => d.StationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
}



