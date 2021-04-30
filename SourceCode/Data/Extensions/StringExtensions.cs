using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Data.Extensions
{
    public static class StringExtensions
    {
        public static bool HasValue([NotNullWhen(true)] this string? me) =>
    !string.IsNullOrWhiteSpace(me);

        public static bool HasNoValue([NotNullWhen(false)] this string? me) =>
            string.IsNullOrWhiteSpace(me);

    }
}
