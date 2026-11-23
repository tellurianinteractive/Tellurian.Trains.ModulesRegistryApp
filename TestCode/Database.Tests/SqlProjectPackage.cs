using System.Xml.Linq;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;

namespace ModulesRegistry.Database.Tests;

/// <summary>
/// Builds the DACPAC from the files that <c>Database.sqlproj</c> includes as <c>Build</c> items, with DacFx in memory.
/// The project itself builds only with MSBuild on Windows; this works on any platform, also in CI.
/// </summary>
public static class SqlProjectPackage
{
    private const string Collation = "Finnish_Swedish_CI_AS";

    public static string ProjectFile()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var project = Path.Combine(directory.FullName, "SourceCode", "Database", "Database.sqlproj");
            if (File.Exists(project)) return project;
            directory = directory.Parent;
        }
        throw new FileNotFoundException("SourceCode/Database/Database.sqlproj was not found above the test's directory.");
    }

    /// <summary>
    /// The script files of the project, in the order the project lists them.
    /// </summary>
    public static IReadOnlyList<string> BuildFiles(string projectFile)
    {
        var projectDirectory = Path.GetDirectoryName(projectFile)!;
        return [.. XDocument.Load(projectFile).Descendants()
            .Where(e => e.Name.LocalName == "Build")
            .Select(e => e.Attribute("Include")?.Value)
            .OfType<string>()
            .Select(include => Path.Combine(projectDirectory, include.Replace('\\', Path.DirectorySeparatorChar)))];
    }

    /// <summary>
    /// The model as the SQL project defines it; throws if a script has errors.
    /// </summary>
    public static TSqlModel CreateModel(string projectFile)
    {
        var model = new TSqlModel(SqlServerVersion.SqlAzure, new TSqlModelOptions { Collation = Collation });
        foreach (var file in BuildFiles(projectFile))
        {
            model.AddObjects(File.ReadAllText(file));
        }
        var errors = model.Validate().Where(m => m.MessageType == DacMessageType.Error).ToList();
        if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(e => e.ToString())));
        return model;
    }

    public static DacPackage Create()
    {
        using var model = CreateModel(ProjectFile());
        var stream = new MemoryStream();
        DacPackageExtensions.BuildPackage(stream, model, new PackageMetadata { Name = "ModulesRegistry" });
        stream.Position = 0;
        return DacPackage.Load(stream, DacSchemaModelStorageType.Memory, FileAccess.Read);
    }
}