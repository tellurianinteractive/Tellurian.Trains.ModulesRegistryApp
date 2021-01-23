using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Data.Validators
{
    public static class ValidatorsExtensions
    {
        public static IRuleBuilderOptions<T, string> NameIsCapitalizedCorrectly<T>(this IRuleBuilder<T, string> builder) =>
            builder.Must(value => value.IsNameCapitalizedCorrectly()).WithMessage("{PropertyName} is not capitalized correctly.");

        public static bool IsNameCapitalizedCorrectly(this string? name)
        {
            if (name is null || name.Length < 1) return true;
            for (var i = 0; i < name.Length; i++)
            {
                if (i == 0 || (name[i - 1] == ' '))
                {
                    if (Char.IsLower(name[i])) return false;
                }
            }
            return true;
        }
    }
}
