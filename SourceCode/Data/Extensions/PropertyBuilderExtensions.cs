using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ModulesRegistry.Data.Extensions;

public static class PropertyBuilderExtensions
{
    extension<TProperty>(PropertyBuilder<TProperty> property)
    {
        /// <summary>
        /// The column's default in the database (as in the SQL project), which EF doesn't rely on: it still writes the property's value on
        /// insert, also <see langword="false"/> or 0 where the default is something else.
        /// </summary>
        public PropertyBuilder<TProperty> HasDatabaseDefault(string sql) => property.HasDefaultValueSql(sql).ValueGeneratedNever();
    }
}