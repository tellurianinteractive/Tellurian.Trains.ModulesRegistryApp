using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Imports.Tests;
using System.Data;

namespace ModulesRegistry.Data.Integration.Tests
{
    [TestClass]
    public class UploadTests
    {

        [Ignore]
        [TestMethod]
        public void UploadNhm()
        {
            const string sourceDatabaseName = @"C:\Users\Stefan\OneDrive\Modelljärnväg\Trafikspel\Vagnskort och fraktsedlar\NMH-koder.accdb";
            const string destinationDatabaseName = "Tellurian.Trains.Database";

            using var sourceConnection = sourceDatabaseName.AsOdbcConnection();
            using var destinationConnection = destinationDatabaseName.AsSqlConnection();
            var sourceCommand = "SELECT * FROM Useful ORDER BY NHMCode8".AsOdbcCommand(sourceConnection);
            sourceConnection.Open();
            destinationConnection.Open();
            var reader = sourceCommand.ExecuteReader(CommandBehavior.CloseConnection);
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var levelDigits = reader.GetInt32(1) * 2;
                var destinationSql = $"INSERT INTO NHM (Id, Code, LevelDigits, DA, DE, EN, NL, PL, SV) VALUES ({id}, '{id:00000000}', {levelDigits}, @DA, @DE, @EN, @NL, @PL, @SV)";
                var destinationCommand = destinationSql.AsSqlCommand(destinationConnection);
                destinationCommand.Parameters.AddWithValue("@DA", reader.GetString(2));
                destinationCommand.Parameters.AddWithValue("@DE", reader.GetString(3));
                destinationCommand.Parameters.AddWithValue("@EN", reader.GetString(4));
                destinationCommand.Parameters.AddWithValue("@NL", reader.GetString(5));
                destinationCommand.Parameters.AddWithValue("@PL", reader.GetString(6));
                destinationCommand.Parameters.AddWithValue("@SV", reader.GetString(7));
                destinationCommand.ExecuteNonQuery();
            }
            destinationConnection.Close();
        }
    }


}
