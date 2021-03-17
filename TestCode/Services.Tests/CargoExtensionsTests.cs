using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System.Globalization;

namespace ModulesRegistry.Services.Tests
{
    [TestClass]
    public class CargoExtensionsTests
    {
        [TestMethod]
        public void WhenNoTextsAreAvailableReturnsNull()
        {
            var target = new Cargo();
            var culture = new CultureInfo("da");
            var actual = culture.LocalizedName(target);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ReturnsSpecificLanguage()
        {
            var target = new Cargo { DA="Dansk tekst."};
            var culture = new CultureInfo("da");
            var actual = culture.LocalizedName(target);
            Assert.IsNotNull(actual);
            Assert.AreEqual("da", actual.Language);
            Assert.AreEqual(target.DA, actual.Value);
        }

        [TestMethod]
        public void ReturnsEnglishIfSpecificLanguageIsNotAvaliable()
        {
            var target = new Cargo { EN = "English text." };
            var culture = new CultureInfo("da");
            var actual = culture.LocalizedName(target);
            Assert.IsNotNull(actual);
            Assert.AreEqual("en", actual.Language);
            Assert.AreEqual(target.EN, actual.Value);
        }
    }
}
