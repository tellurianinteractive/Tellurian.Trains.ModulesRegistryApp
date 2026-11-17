using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Extensions;
using System.Security.Claims;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The recipients of the user administration page's mail buttons (<see cref="UserMailRecipientsExtensions"/>).
/// </summary>
[TestClass]
public sealed class UserMailRecipientsTests
{
    private const int Sweden = 1;
    private const int Germany = 2;
    private static readonly DateTimeOffset Registered = new(2024, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Since = new(2025, 11, 17, 0, 0, 0, TimeSpan.Zero);

    private static User UserOf(int id, int countryId, string? email = "x@example.com")
    {
        var user = new User { Id = id, RegistrationTime = Registered };
        user.Person = new Person { Id = id, FirstName = $"P{id}", LastName = "L", CountryId = countryId, EmailAddresses = email!, User = user, UserId = id };
        return user;
    }

    private static ClaimsPrincipal Principal(bool isGlobal, int countryId) =>
        new(new ClaimsIdentity(
            isGlobal ?
                [new Claim(AppClaimTypes.GlobalAdministrator, "true"), new Claim(AppClaimTypes.CountryId, countryId.ToString())] :
                [new Claim(AppClaimTypes.CountryAdministrator, "true"), new Claim(AppClaimTypes.CountryId, countryId.ToString())],
            "Test"));

    private static List<User> Administrators()
    {
        var global = UserOf(1, Sweden); global.IsGlobalAdministrator = true;
        var both = UserOf(2, Germany); both.IsGlobalAdministrator = true; both.IsCountryAdministrator = true;
        var swedish = UserOf(3, Sweden); swedish.IsCountryAdministrator = true;
        var german = UserOf(4, Germany); german.IsCountryAdministrator = true;
        var noEmail = UserOf(5, Sweden, email: null); noEmail.IsCountryAdministrator = true;
        return [global, both, swedish, german, noEmail];
    }

    [TestMethod]
    public void GlobalAdministratorGetsAllAdministratorsWithEmail()
    {
        var ids = Administrators().AdministratorsFor(Principal(isGlobal: true, Sweden)).Select(p => p.Id).ToArray();
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, ids);
    }

    [TestMethod]
    public void CountryAdministratorGetsGlobalAdministratorsAndCountryAdministratorsInOwnCountry()
    {
        var ids = Administrators().AdministratorsFor(Principal(isGlobal: false, Sweden)).Select(p => p.Id).ToArray();
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, ids);
    }

    [TestMethod]
    public void NotSignedInSinceSelectsSignedInAndPasswordCreatedBeforeTheDate()
    {
        var signedInBefore = UserOf(1, Sweden); signedInBefore.LastSignInTime = Since.AddDays(-1);
        var signedInAfter = UserOf(2, Sweden); signedInAfter.LastSignInTime = Since.AddDays(1);
        var passwordBefore = UserOf(3, Sweden); passwordBefore.HashedPassword = "hash"; passwordBefore.LastEmailConfirmationTime = Since.AddDays(-1);
        var passwordAfter = UserOf(4, Sweden); passwordAfter.HashedPassword = "hash"; passwordAfter.LastEmailConfirmationTime = Since.AddDays(1);
        var onlyInvited = UserOf(5, Sweden);
        var noEmail = UserOf(6, Sweden, email: null); noEmail.LastSignInTime = Since.AddDays(-1);
        var noUser = new Person { Id = 7, FirstName = "P7", CountryId = Sweden, EmailAddresses = "y@example.com" };
        Person[] people = [signedInBefore.Person, signedInAfter.Person, passwordBefore.Person, passwordAfter.Person, onlyInvited.Person, noEmail.Person, noUser];

        var ids = people.NotSignedInSince(Since).Select(p => p.Id).ToArray();

        CollectionAssert.AreEquivalent(new[] { 1, 3 }, ids);
    }
}