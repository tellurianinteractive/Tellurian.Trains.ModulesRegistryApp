using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class PasswordTests
{
    [TestMethod]
    public void IsValidPasswords()
    {
        var policy = new PasswordPolicy();
        Assert.IsTrue(policy.IsValid("FonjMonj899cc!"));
    }

    [TestMethod]
    public void CreatePasswordHash()
    {
        const string Password = "secret_password";
        var hashedPassword = Password.AsHashedPassword();
        Assert.IsNotNull(hashedPassword);
    }
}
