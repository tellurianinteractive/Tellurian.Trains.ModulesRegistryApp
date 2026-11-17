using ModulesRegistry.Data;
using ModulesRegistry.Extensions;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The persons list's status icon and date (<see cref="PersonUserStatusExtensions"/>): the red states come first and
/// always show their own icon; an administrator otherwise gets the role's icon; the date belongs to the state.
/// </summary>
[TestClass]
public sealed class PersonUserStatusTests
{
    private static readonly DateTimeOffset Registered = new(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Confirmed = new(2026, 2, 3, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset SignedIn = new(2026, 3, 4, 10, 0, 0, TimeSpan.Zero);

    private static Person PersonWith(User? user, string? email = "anna@example.com") =>
        new() { Id = 1, FirstName = "Anna", LastName = "Svensson", EmailAddresses = email!, User = user!, UserId = user?.Id };

    private static User ActiveUser() =>
        new() { Id = 10, RegistrationTime = Registered, LastSignInTime = SignedIn, LastTermsOfUseAcceptTime = SignedIn };

    [TestMethod]
    public void SignedInUserShowsTheLastSignInDate()
    {
        var status = PersonWith(ActiveUser()).AccountStatus();

        Assert.AreEqual(PersonUserState.SignedIn, status.State);
        Assert.AreEqual("fa fa-user-check", status.IconClass);
        Assert.AreEqual(SignedIn, status.Date);
        Assert.AreEqual("LastLoginTime", status.DateTextKey);
        Assert.IsFalse(status.IsWarning);
    }

    [TestMethod]
    public void LockedOutIsRedWithoutDateAlsoForAnAdministrator()
    {
        var user = ActiveUser();
        user.FailedLoginAttempts = 99;
        user.IsGlobalAdministrator = true;

        var status = PersonWith(user).AccountStatus();

        Assert.AreEqual(PersonUserState.LockedOut, status.State);
        Assert.AreEqual("fa fa-user-lock", status.IconClass);
        Assert.IsTrue(status.IsWarning);
        Assert.IsNull(status.Date);
        Assert.IsNull(status.RoleTextKey);
    }

    [TestMethod]
    public void SignedInWithoutAcceptingTheTermsIsRedAlsoForAnAdministrator()
    {
        var user = ActiveUser();
        user.LastTermsOfUseAcceptTime = null;
        user.IsCountryAdministrator = true;

        var status = PersonWith(user).AccountStatus();

        Assert.AreEqual(PersonUserState.NotAcceptedTermsOfUse, status.State);
        Assert.AreEqual("fa fa-file-signature", status.IconClass);
        Assert.IsTrue(status.IsWarning);
        Assert.IsNull(status.Date);
    }

    [TestMethod]
    public void CreatedPasswordShowsTheKeyAndTheConfirmationDate()
    {
        var user = new User { Id = 10, RegistrationTime = Registered, HashedPassword = "hash", LastEmailConfirmationTime = Confirmed };

        var status = PersonWith(user).AccountStatus();

        Assert.AreEqual(PersonUserState.PasswordCreated, status.State);
        Assert.AreEqual("fa fa-key", status.IconClass);
        Assert.AreEqual(Confirmed, status.Date);
        Assert.AreEqual("PasswordCreated", status.DateTextKey);
    }

    [TestMethod]
    public void InvitedShowsTheEnvelopeAndTheInvitationDate()
    {
        var status = PersonWith(new User { Id = 10, RegistrationTime = Registered }).AccountStatus();

        Assert.AreEqual(PersonUserState.Invited, status.State);
        Assert.AreEqual("fa fa-envelope", status.IconClass);
        Assert.AreEqual(Registered, status.Date);
        Assert.AreEqual("Invited", status.DateTextKey);
    }

    [TestMethod]
    public void AdministratorGetsTheRoleIconAndKeepsTheStateDate()
    {
        var user = ActiveUser();
        user.IsGlobalAdministrator = true;

        var status = PersonWith(user).AccountStatus();

        Assert.AreEqual(PersonUserState.SignedIn, status.State);
        Assert.AreEqual("fa fa-user-shield", status.IconClass);
        Assert.AreEqual("GlobalAdministrator", status.RoleTextKey);
        Assert.AreEqual(SignedIn, status.Date);
    }

    [TestMethod]
    public void PersonWithoutEmailOrUser()
    {
        Assert.AreEqual(PersonUserState.NoEmail, PersonWith(null, email: null).AccountState());
        var noUser = PersonWith(null).AccountStatus();
        Assert.AreEqual(PersonUserState.NoUser, noUser.State);
        Assert.AreEqual("fa fa-user-slash", noUser.IconClass);
        Assert.IsNull(noUser.Date);
    }
}