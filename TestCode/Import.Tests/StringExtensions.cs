using Microsoft.Data.SqlClient;
using System;
using System.Data.Odbc;

namespace ModulesRegistry.Imports.Tests
{
    public static class StringExtensions
    {
        public static OdbcConnection AsOdbcConnection(this string databaseFileName)
        {
            const string driver = "{Microsoft Access Driver (*.mdb, *.accdb)}";
            var connectionString = $"Driver={driver};DBQ={databaseFileName}";
            return new (connectionString);
        }
        public static OdbcCommand AsOdbcCommand(this string sql, OdbcConnection connection) => new(sql, connection);

        public static SqlConnection AsSqlConnection(this string databaseName) =>
            databaseName switch
            {
                "TimetablePlanning" => new ($"Server=localhost;Database={databaseName};Trusted_Connection=True;"),
                _ => throw new NotSupportedException(databaseName)
            };

        public static SqlCommand AsSqlCommand(this string sql, SqlConnection connection) => new (sql, connection);
    }
}
