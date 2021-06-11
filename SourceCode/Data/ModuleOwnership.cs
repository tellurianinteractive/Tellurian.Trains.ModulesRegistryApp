using System;
using System.Collections.Generic;
using Rationals;

#nullable disable

namespace ModulesRegistry.Data
{
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

    public static class ModuleOwnershipExtensions
    {
        public static string OwnerName(this ModuleOwnership? me) =>
            me is null ? string.Empty :
            me.Group is not null ? me.Group.FullName :
            me.Person is not null ? me.Person.FullName() :
            me.GroupId is not null ? $"Group {me.GroupId}" :
            me.PersonId is not null ? $"Person {me.PersonId}" :
            "?";

        public static string OwnedShareAndPercentage(this ModuleOwnership? me) =>
            me is null ? string.Empty :
            $"{me.OwnedShare()} ({me.OwnedPercent()*100:F1}%)";

        public static double OwnedPercent(this ModuleOwnership? me) =>
            me is null || me.OwnedShare == 0 ? 0.0 :
            me.OwnedShare;

        public static Rational OwnedShare(this ModuleOwnership? me) =>
            me is null ? Rational.Zero :
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
    }
}
