using ModulesRegistry.Security;

namespace ModulesRegistry.AppTests.Security;

/// <summary>
/// The invitation mail tells how long the link works (decided 2026-11-21), in every language, with the same number of days as
/// <see cref="InvitationTokens.Lifetime"/>: change both together.
/// </summary>
[TestClass]
public sealed class InvitationTextTests
{
    [TestMethod]
    [DataRow("Invitation.md")]
    [DataRow("Invitation.sv.md")]
    [DataRow("Invitation.da.md")]
    [DataRow("Invitation.nb.md")]
    [DataRow("Invitation.de.md")]
    public void TheMailTellsHowLongTheLinkWorks(string file)
    {
        var text = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Content", "Markdown", file));
        StringAssert.Contains(text, $" {InvitationTokens.Lifetime.TotalDays:0} ", $"{file} doesn't say {InvitationTokens.Lifetime.TotalDays:0} days.");
    }
}