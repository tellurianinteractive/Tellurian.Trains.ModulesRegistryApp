using Microsoft.EntityFrameworkCore.Diagnostics;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Security;
using ModulesRegistry.Services.Implementations;
using System.Data.Common;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The claims transformation runs for every request from a signed-in user, so it must read everything in one query.
/// </summary>
[TestClass]
public sealed class ApplicationClaimsTransformationTests
{
    private const int CountryId = 1;
    private const int UserId = 7;
    private const int PersonId = 70;
    private static readonly Guid ObjectId = Guid.Parse("3411ef21-3d68-4200-a4a0-5aa922d332a1");

    private SqliteDbContextFactory _factory = null!;
    private QueryCounter _queries = null!;
    private DirectoryInfo _contentDirectory = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _queries = new QueryCounter();
        _factory = new SqliteDbContextFactory(_queries);
        _contentDirectory = Directory.CreateTempSubdirectory("ClaimsTransformationTests");
        await _factory.SeedAsync(new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
        await _factory.SeedAsync(
            new User
            {
                Id = UserId, ObjectId = ObjectId, EmailAddress = "user@example.com", HashedPassword = "x",
                IsCountryAdministrator = true, LastTermsOfUseAcceptTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            },
            new GroupDomain { Id = 5, Name = "Domain 5" },
            new GroupDomain { Id = 6, Name = "Domain 6" });
        await _factory.SeedAsync(
            new Person { Id = PersonId, UserId = UserId, FirstName = "Anna", LastName = "Berg", CityName = "City", CountryId = CountryId },
            new Person { Id = 71, FirstName = "Other", LastName = "Person", CityName = "City", CountryId = CountryId },
            NewGroup(1, domainId: 5), NewGroup(2, domainId: 5), NewGroup(3, domainId: null), NewGroup(4, domainId: 6));
        await _factory.SeedAsync(
            new GroupMember { Id = 1, GroupId = 1, PersonId = PersonId },
            new GroupMember { Id = 2, GroupId = 2, PersonId = PersonId },
            new GroupMember { Id = 3, GroupId = 3, PersonId = PersonId },
            new GroupMember { Id = 4, GroupId = 4, PersonId = 71 });
        _queries.Count = 0;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _factory.Dispose();
        _contentDirectory.Delete(recursive: true);
    }

    [TestMethod]
    public async Task AddsTheUsersClaimsInOneQuery()
    {
        var actual = await CreateTarget().TransformAsync(SignedIn(ObjectId));

        Assert.AreEqual(1, _queries.Count, "One round trip to the database.");
        Assert.AreEqual(UserId, actual.UserId());
        Assert.AreEqual(PersonId, actual.PersonId());
        Assert.AreEqual(CountryId, actual.CountryId());
        Assert.AreEqual("Anna", actual.GivenName());
        Assert.AreEqual("Berg", actual.Surname());
        Assert.IsTrue(actual.IsCountryAdministrator());
        Assert.IsFalse(actual.IsGlobalAdministrator());
        Assert.IsTrue(actual.IsLatestTermsOfUseAccepted());
        // Domain 5 once (two groups), not domain 6 (another person's group) and nothing for the group without a domain.
        CollectionAssert.AreEqual(new[] { "5" }, actual.FindAll(AppClaimTypes.DomainId).Select(c => c.Value).ToArray());
    }

    [TestMethod]
    public async Task UserWithoutPersonGetsOnlyUserClaims()
    {
        await using (var db = _factory.CreateDbContext())
        {
            db.People.Single(p => p.Id == PersonId).UserId = null;
            await db.SaveChangesAsync();
        }
        _queries.Count = 0;

        var actual = await CreateTarget().TransformAsync(SignedIn(ObjectId));

        Assert.AreEqual(1, _queries.Count);
        Assert.AreEqual(UserId, actual.UserId());
        Assert.IsFalse(actual.HasClaim(c => c.Type == AppClaimTypes.PersonId));
        Assert.IsFalse(actual.HasClaim(c => c.Type == AppClaimTypes.DomainId));
    }

    [TestMethod]
    public async Task UnknownUserIsLeftAsIs()
    {
        var principal = SignedIn(Guid.NewGuid());

        var actual = await CreateTarget().TransformAsync(principal);

        Assert.AreSame(principal, actual);
    }

    private ApplicationClaimsTransformation CreateTarget() =>
        new(_factory, new ContentService(null!, _contentDirectory.FullName), new AppService());

    private static ClaimsPrincipal SignedIn(Guid objectId) =>
        new(new ClaimsIdentity([new Claim(AppClaimTypes.ObjectId, objectId.ToString())], "Test"));

    private static Group NewGroup(int id, int? domainId) =>
        new() { Id = id, CountryId = CountryId, FullName = $"Group {id}", ShortName = $"G{id}", Category = "Club", CityName = "City", GroupDomainId = domainId };

    /// <summary>
    /// Counts the queries (commands that read data).
    /// </summary>
    private sealed class QueryCounter : DbCommandInterceptor
    {
        public int Count { get; set; }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Count++;
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Count++;
            return ValueTask.FromResult(result);
        }
    }
}