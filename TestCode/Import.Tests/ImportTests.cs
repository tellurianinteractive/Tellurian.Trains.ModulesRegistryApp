using AutoMapper;
using AutoMapper.Data;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Odbc;
using System.Globalization;

#pragma warning disable IDE0060 // Remove unused parameter

namespace ModulesRegistry.Data.Integration;

[Ignore("Should only be run to import test data.")]
[TestClass]
public class ImportTests
{
    [TestMethod]
    public void ImportCountries()
    {
        Import<Country>("SELECT * FROM Country", LocalDestination);
    }

    public static void Import<T>(string originSql, Func<SqlConnection> getOriginConnection)
    {
        //using var origin = getOriginConnection();
        //var command = new SqlCommand(originSql, origin);
        //origin.Open();
        //var reader = command.ExecuteReader();

        //var mapper = CreateMapper<T>();
        //using var db = new ModulesDbContext();
        //while (reader.Read())
        //{
        //    var model = mapper.Map<T>(reader);
        //    db.Add(model);
        //}
        //db.Database.OpenConnection();
        //try
        //{
        //    db.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT dbo.{typeof(T).Name} ON");
        //    db.SaveChanges();
        //    db.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT dbo.{typeof(T).Name} OFF");
        //}
        //finally
        //{
        //    db.Database.CloseConnection();
        //}
    }

    internal static OdbcConnection CreateModuleConnection() =>
        CreateSourceConnection(@"C:\Users\Stefan\OneDrive\Modelljärnväg\Träffar\2020\2020-05 Gävle\Banplanering\Anmälda moduler.accdb");
    internal static OdbcConnection CreatePlanningConnection() =>
        CreateSourceConnection(@"C:\Users\Stefan\OneDrive\Modelljärnväg\Träffar\2020\2020-10 Värnamo\Trafikplanering\Timetable.accdb");

    internal static OdbcConnection CreateSourceConnection(string databaseFileName)
    {
        const string driver = "{Microsoft Access Driver (*.mdb, *.accdb)}";
        var connectionString = string.Format(CultureInfo.InvariantCulture, "Driver={0};DBQ={1}", driver, databaseFileName);
        return new OdbcConnection(connectionString);
    }

    internal static SqlConnection LocalDestination() =>
        new("Server=localhost;Database=TimetablePlanning;Trusted_Connection=True;");

    internal static IMapper CreateMapper<T>()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddDataReaderMapping();
            cfg.CreateMap<IDataRecord, T>();
        });
        config.AssertConfigurationIsValid();
        return config.CreateMapper();
    }
}
