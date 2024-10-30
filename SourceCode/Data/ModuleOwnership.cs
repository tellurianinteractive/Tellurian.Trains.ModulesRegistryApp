using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Rationals;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace ModulesRegistry.Data;

public partial class ModuleOwnership
{
    public int Id { get; set; }
    public int? PersonId { get; set; }
    public int? GroupId { get; set; }
    public int ModuleId { get; set; }
    public double OwnedShare { get; set; }

    public virtual Group Group { get; set; }
    public virtual Module Module { get; set; }
    public virtual Person Person { get; set; }
}

#nullable enable

public enum GroupOwnershipType
{
    Unknown = 0,
    Personal = 1,
    Group = 2,
    Combined = 3,
}

public record ModuleOwnershipTransfer(ModuleOwnershipRef OldOwnerRef, ModuleOwnershipRef NewOwnerRef, double TransferredPercent)
{
    public ModuleOwnershipTransfer(int oldOwnerPersonId, int newOwnerPersonId, double transferredPercent) :
        this(ModuleOwnershipRef.Person(oldOwnerPersonId), ModuleOwnershipRef.Person(newOwnerPersonId), transferredPercent)
    { }
    public bool IsZero => TransferredPercent == 0;
}

public static class ModuleOwnershipTransferExtensions
{
    public static (ModuleOwnership From, ModuleOwnership To) Transfer(this ModuleOwnership original, ModuleOwnershipTransfer transfer)
    {

        if (original.AsModuleOwnershipRef() == transfer.NewOwnerRef)
        {
            original.OwnedShare = Math.Min(1, original.OwnedShare + transfer.TransferredPercent);
            return (original, original.WithNewOwner(transfer.NewOwnerRef));
        }
        var newOwnership = original.WithNewOwner(transfer.NewOwnerRef);
        newOwnership.OwnedShare = Math.Min(original.OwnedShare, transfer.TransferredPercent);
        original.OwnedShare = Math.Max(0, original.OwnedShare - transfer.TransferredPercent);
        return (original, newOwnership );
    }
}

public static class ModuleOwnershipExtensionsk
{
    public static bool ISOwnedByPerson(this ModuleOwnership ownership,int personId) => ownership.OwnedShare > 0 && ownership.PersonId == personId;
    public static bool IsOwnedByGroup(this ModuleOwnership ownership,int groupId) => ownership.OwnedShare > 0 && ownership.GroupId == groupId;
    public static ModuleOwnership WithNewOwner(this ModuleOwnership original, ModuleOwnershipRef newOwnerRef) =>
        new()
        {
            ModuleId = original.ModuleId,
            PersonId = newOwnerRef.PersonId > 0 ? newOwnerRef.PersonId : null,
            GroupId = newOwnerRef.GroupId > 0 ? newOwnerRef.GroupId: null,
            OwnedShare = 0
        };
    public static ModuleOwnershipRef AsModuleOwnershipRef(this ModuleOwnership? ownership) =>
        ownership is null ? ModuleOwnershipRef.None :
        ModuleOwnershipRef.PersonAndOrGroup(ownership.PersonId ?? 0, ownership.GroupId ?? 0);
    public static bool HasPersonOrGroupOwner([NotNullWhen(true)] this ModuleOwnership? ownership) =>
        ownership is not null && (ownership.PersonId.HasValue || ownership.GroupId.HasValue);

    public static bool IsAssistantOnly([NotNullWhen(true)] this ModuleOwnership? ownership) =>
        ownership is not null && ownership.OwnedShare == 0 && ownership.PersonId > 0;

    public static string OwnerNames(this IEnumerable<ModuleOwnership> us) =>
        string.Join(", ", us.First().Module.ModuleOwnerships.Select(mo => mo.OwnerName()));

    public static string OwnerName(this ModuleOwnership? me) =>
        me is null ? string.Empty :
        me.Group is not null ? me.Group.FullName :
        me.Person is not null ? me.Person.Name() :
        me.GroupId is not null ? $"Group {me.GroupId}" :
        me.PersonId is not null ? $"Person {me.PersonId}" :
        "?";

    public static string OwnedShareAndPercentage(this ModuleOwnership? me, IStringLocalizer localizer) =>
        me is null ? string.Empty :
        me.OwnedShare == 0 ? localizer["AssistantOnly"].Value :
        $"{me.OwnedShare()} ({me.OwnedPercent() * 100:F1}%)";

    public static double OwnedPercent(this ModuleOwnership? me) =>
        me is null || me.OwnedShare == 0 ? 0.0 :
        me.OwnedShare;

    public static Rational OwnedShare(this ModuleOwnership? me) =>
        me is null || me.OwnedShare == 0 ? Rational.Zero :
        Rational.Approximate(me.OwnedShare, 0.01);

    public static (bool Ok, double Percentage) AddShare(this ModuleOwnership me, Rational share)
    {
        var value = me.OwnedShare() + share;
        if (value > Rational.One) return (false, (double)value);
        return (true, (double)value);
    }

    public static (bool Ok, double Percentage) SubtractShare(this ModuleOwnership me, Rational share)
    {
        var value = me.OwnedShare() - share;
        if (value < Rational.Zero) return (false, (double)value);
        return (true, (double)value);
    }

    public static GroupOwnershipType OwnershipType(this ModuleOwnership me) =>
        me.PersonId > 0 && me.GroupId > 0 ? GroupOwnershipType.Combined :
        me.PersonId > 0 ? GroupOwnershipType.Personal :
        me.GroupId > 0 ? GroupOwnershipType.Group :
        GroupOwnershipType.Unknown;
}

public static class ModuleOwnershipMapping
{
    internal static void MapModuleOwnership(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ModuleOwnership>(entity =>
        {
            entity.ToTable("ModuleOwnership");

            entity.HasOne(d => d.Group)
                .WithMany(p => p.ModuleOwnerships)
                .HasForeignKey(d => d.GroupId);

            entity.HasOne(d => d.Module)
                .WithMany(p => p.ModuleOwnerships)
                .HasForeignKey(d => d.ModuleId);

            entity.HasOne(d => d.Person)
                .WithMany(p => p.ModuleOwnerships)
                .HasForeignKey(d => d.PersonId);
        });
}