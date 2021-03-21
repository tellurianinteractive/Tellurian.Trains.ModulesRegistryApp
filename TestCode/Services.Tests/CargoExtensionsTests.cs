using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Data;
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
            var actual = target.LocalizedName(culture);
            Assert.AreEqual("", actual.Value);
            Assert.AreEqual("", actual.Language);
        }

        [TestMethod]
        public void ReturnsSpecificLanguage()
        {
            var target = new Cargo { DA="Dansk tekst."};
            var culture = new CultureInfo("da");
            var actual = target.LocalizedName(culture);
            Assert.IsNotNull(actual);
            Assert.AreEqual("da", actual.Language);
            Assert.AreEqual(target.DA, actual.Value);
        }

        [TestMethod]
        public void ReturnsEnglishIfSpecificLanguageIsNotAvaliable()
        {
            var target = new Cargo { EN = "English text." };
            var culture = new CultureInfo("da");
            var actual = target.LocalizedName(culture);
            Assert.IsNotNull(actual);
            Assert.AreEqual("en", actual.Language);
            Assert.AreEqual(target.EN, actual.Value);
        }
    }
}
