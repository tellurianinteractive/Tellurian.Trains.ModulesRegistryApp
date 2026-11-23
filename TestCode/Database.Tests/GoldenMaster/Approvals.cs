namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// Compares a result with its approved file, <c>GoldenMaster/Approved/&lt;name&gt;.approved.txt</c>.
/// When they differ, or no approved file exists, the result is written next to it as <c>&lt;name&gt;.received.txt</c> (git-ignored)
/// and the test fails. Review the received file; if it is right, rename it to <c>.approved.txt</c>.
/// </summary>
public static class Approvals
{
    public static void Verify(string name, string actual)
    {
        var directory = RepositoryFiles.PathOf("TestCode", "Database.Tests", "GoldenMaster", "Approved");
        var approvedFile = Path.Combine(directory, $"{name}.approved.txt");
        var receivedFile = Path.Combine(directory, $"{name}.received.txt");
        var normalized = Normalized(actual);
        var approved = File.Exists(approvedFile) ? Normalized(File.ReadAllText(approvedFile)) : null;
        if (approved == normalized)
        {
            if (File.Exists(receivedFile)) File.Delete(receivedFile);
            return;
        }
        Directory.CreateDirectory(directory);
        File.WriteAllText(receivedFile, normalized.Replace("\n", "\r\n"));
        Assert.Fail(approved is null
            ? $"No approved result for '{name}'. Review {receivedFile} and rename it to .approved.txt."
            : $"The result for '{name}' differs from the approved one. Compare {receivedFile} with {approvedFile}.");
    }

    private static string Normalized(string text) => text.Replace("\r\n", "\n").TrimEnd('\n');
}