using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Tests
{
    [TestClass]
    public class PasswordTests
    {
        [TestMethod]
        public void IsValidPasswords()
        {
            var policy = new PasswordPolicy();
            Assert.IsTrue(policy.IsValid("DonjDonj891mm!"));
        }
    }
}
