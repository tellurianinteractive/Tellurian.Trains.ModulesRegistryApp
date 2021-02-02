using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Extensions
{
    static public class DbContextExtensions
    {
        public static EntityState GetState(this int id) => id > 0 ? EntityState.Modified : EntityState.Added;
       public static bool IsNotSet([NotNullWhen(false )] this int? me) => me is null || me <= 0;
        public static bool IsSet([NotNullWhen(true)] this int? me) => me is not null && me > 0;

    }
}
