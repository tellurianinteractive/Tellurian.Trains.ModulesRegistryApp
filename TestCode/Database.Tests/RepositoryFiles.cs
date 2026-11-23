namespace ModulesRegistry.Database.Tests;

/// <summary>
/// Files in the repository, found above the test's output directory, so tests read and write the checked-in files.
/// </summary>
public static class RepositoryFiles
{
    public static string Root()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Modules Registry App.slnx"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("The repository root (Modules Registry App.slnx) was not found above the test's directory.");
    }

    public static string PathOf(params string[] parts) => Path.Combine([Root(), .. parts]);
}