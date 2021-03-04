using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModulesRegistry.Services.Tests
{
    [TestClass]
    public class PasswordTests
    {
        [TestMethod]
        public void IsValidPasswords()
        {
            var policy = new PasswordPolicy();
            Assert.IsTrue(policy.IsValid("FonjMonj899cc!"));
        }
    }
}
