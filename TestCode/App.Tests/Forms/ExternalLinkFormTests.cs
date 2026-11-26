using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.ExternalLinks;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The external link form: the mapping to and from the entity, and its DataAnnotations run as the server runs them.
/// </summary>
[TestClass]
public sealed class ExternalLinkFormTests
{
    private static ExternalLink Existing() => new()
    {
        Id = 5, OwnerType = ExternalLinkOwners.ModuleStandard, OwnerId = 2, Purpose = ExternalLinkPurpose.StandardDocument,
        Href = "https://fremo-net.eu/norm.pdf", ContentType = ExternalLinkContentTypes.Pdf, Language = "de", IsDefault = true,
    };

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(expected.OwnerType, expected.OwnerId, expected.Id);
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.OwnerType, actual.OwnerType);
        Assert.AreEqual(expected.OwnerId, actual.OwnerId);
        Assert.AreEqual(expected.Purpose, actual.Purpose);
        Assert.AreEqual(expected.Href, actual.Href);
        Assert.AreEqual(expected.ContentType, actual.ContentType);
        Assert.AreEqual(expected.Language, actual.Language);
        Assert.AreEqual(expected.IsDefault, actual.IsDefault);
    }

    [TestMethod]
    public void AValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(Existing().AsFormData()));

    [TestMethod]
    public void RequiredFields() =>
        CollectionAssert.AreEquivalent(new[] { "Purpose", "Href", "ContentType" }, InvalidMembers(new ExternalLinkFormData()), "No language is a choice.");

    [TestMethod]
    [DataRow("fremo-net.eu/norm.pdf", DisplayName = "No scheme")]
    [DataRow("ftp://fremo-net.eu/norm.pdf", DisplayName = "Another scheme")]
    public void TheAddressIsAWebAddress(string href)
    {
        var form = Existing().AsFormData();
        form.Href = href;
        CollectionAssert.AreEqual(new[] { "Href" }, InvalidMembers(form));
    }

    [TestMethod]
    public void TheAddressHasAMaximumLength()
    {
        var form = Existing().AsFormData();
        form.Href = "https://example.org/" + new string('x', ExternalLinkExtensions.HrefMaxLength);
        CollectionAssert.AreEqual(new[] { "Href" }, InvalidMembers(form));
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}