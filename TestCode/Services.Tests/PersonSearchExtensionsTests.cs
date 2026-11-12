using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The person search rule, run in memory. In the database the same expression becomes SQL <c>LIKE</c>, and the
/// collation makes it case-insensitive; in memory it is case-sensitive, so the test data uses matching case.
/// </summary>
[TestClass]
public class PersonSearchExtensionsTests
{
    private static readonly Person[] Persons =
    [
        new() { Id = 1, FirstName = "Karl", LastName = "Eliasson", CityName = "Göteborg" },
        new() { Id = 2, FirstName = "Karl", LastName = "Andersson", CityName = "Malmö" },
        new() { Id = 3, FirstName = "Eli", LastName = "Berg", CityName = "Lund" },
        new() { Id = 4, FirstName = "Anna", MiddleName = "Karin", LastName = "Svensson", CityName = null },
    ];

    [TestMethod]
    [DataRow("Kar Eli", new[] { 1 })]
    [DataRow("Eli Kar", new[] { 1 })]
    [DataRow("Karl Göte", new[] { 1 })]
    [DataRow("Kar", new[] { 1, 2, 4 })] // Also Anna Karin (middle name).
    [DataRow("Kari", new[] { 4 })]
    [DataRow("sson Mal", new[] { 2 })]
    [DataRow("Kar Xyz", new int[0])]
    public void EveryWordMustMatchNameOrCity(string text, int[] expectedIds)
    {
        var actual = Persons.AsQueryable().WhereNameOrCityContainsAllWords(PersonSearchExtensions.SearchWords(text)).Select(p => p.Id).ToArray();

        CollectionAssert.AreEquivalent(expectedIds, actual);
    }

    [TestMethod]
    public void HandlesMissingMiddleNameAndCity()
    {
        var actual = Persons.AsQueryable().WhereNameOrCityContainsAllWords(["Svensson"]).Select(p => p.Id).ToArray();

        CollectionAssert.AreEqual(new[] { 4 }, actual);
    }

    /// <summary>
    /// The filter must be translated to SQL (not evaluated in memory). ToQueryString builds the SQL without a database connection.
    /// </summary>
    [TestMethod]
    public void IsTranslatedToSqlLike()
    {
        var options = new DbContextOptionsBuilder<ModulesDbContext>().UseSqlServer("Server=none;Database=none").Options;
        using var dbContext = new ModulesDbContext(options);

        var sql = dbContext.People.WhereNameOrCityContainsAllWords(["kar", "eli"]).ToQueryString();

        Assert.Contains("LIKE", sql);
        Assert.Contains("[FirstName]", sql);
        Assert.Contains("[CityName]", sql);
    }

    [TestMethod]
    [DataRow("kar eli", new[] { "kar", "eli" })]
    [DataRow("  kar   eli  ", new[] { "kar", "eli" })]
    [DataRow("karl", new[] { "karl" })]
    [DataRow("", new string[0])]
    [DataRow(null, new string[0])]
    public void SplitsSearchTextIntoWords(string? text, string[] expected)
    {
        CollectionAssert.AreEqual(expected, PersonSearchExtensions.SearchWords(text));
    }
}