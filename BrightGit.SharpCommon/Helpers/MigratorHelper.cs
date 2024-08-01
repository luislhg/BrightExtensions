using System.Globalization;

namespace BrightGit.SharpCommon.Helpers;
public static class MigratorHelper
{
    public static List<string> FindMigrationsInDir(string migrationDir)
    {
        // Get all .cs files in the Migrations directory (excluding .designer.cs).
        var migrations = Directory.GetFiles(migrationDir)
                                  .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                                              !p.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase))
                                  .OrderBy(p => p)
                                  .ToList();

        // Check only for .cs files containing this specific date format (a real migration).
        string dateformat = "yyyyMMddHHmmss";
        migrations.RemoveAll(migration => !DateTime.TryParseExact(Path.GetFileName(migration)[..14], dateformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));

        return migrations;
    }

    public static string GetLatestCommonNameBetweenTwoLists(List<string> list1, List<string> list2)
    {
        // Get the latest common name between two lists.
        return list1.Intersect(list2).LastOrDefault();
    }

    public static string GetMigrationsDirectory(string repoDir)
    {
        // Check if the repoDir exists.
        if (!Directory.Exists(repoDir))
            return null;

        // Search for the Migrations directory.
        var directories = Directory.GetDirectories(repoDir, "Migrations", SearchOption.AllDirectories);

        // Return the first found Migrations directory, or null if not found.
        return directories.Length > 0 ? directories[0] : null;
    }

    public static string GetProjectFilePathFromInsideOut(string childrDir)
    {
        string currentDir = childrDir;

        while (currentDir != null)
        {
            var projectFile = Directory.GetFiles(currentDir, "*.csproj").FirstOrDefault();
            if (projectFile != null)
                return projectFile;

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        return null;
    }
}
