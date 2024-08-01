using BrightGit.SharpCommon.Helpers;
using LibGit2Sharp;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace EFCoreGitAutoMigration;
public class Program
{
    static async Task Main(string[] args)
    {
        // Skip the git hook to avoid entering an infinite loop.
        var skipVariable = Environment.GetEnvironmentVariable("BRIGHT_GITEFCORE_SKIP_GIT_HOOK");
        if (skipVariable == "1")
            return;

        // Start the app!
        var sw = Stopwatch.StartNew();
        Print($"-- Bright Git-EFCore: Starting at {DateTime.Now} (v{Assembly.GetExecutingAssembly().GetName().Version})");

        // Check if the arguments are correct.
        if (args.Length < 3)
        {
            Print("ERROR: Invalid number of arguments", 0);
            Console.ReadKey();
            return;
        }

        // Read arguments.
        string oldRef = args[0];
        string newRef = args[1];
        int flag = int.Parse(args[2]);
        Print($"Arguments: {oldRef} to {newRef} - flag {flag}", 0);

        // Read current directory.
        var currentDir = Directory.GetCurrentDirectory();
        Print($"Repository Directory: {currentDir}", 0);

        // Exits if flag is 0.
        if (flag == 0)
        {
            return;
        }

        // Check if the current directory is a git repository.
        if (Directory.GetDirectories(currentDir, ".git").Length == 0)
        {
            Print("ERROR: Not a git repository", 0);
            Print("-- TERMINATING Script");
            return;
        }

        // Get migrations directory.
        string migrationDir = MigratorHelper.GetMigrationsDirectory(currentDir);
        Print($"Migrations Directory: {migrationDir}", 0);
        if (migrationDir == null)
        {
            Print("ERROR: Migrations directory not found", 0);
            Print("-- TERMINATING Script");
            return;
        }

        // Get migrations project file and directory.
        string migrationProjectPath = MigratorHelper.GetProjectFilePathFromInsideOut(migrationDir);
        Print($"Migrations Project Path: {migrationProjectPath}", 0);
        if (migrationProjectPath == null)
        {
            Print("ERROR: Project file not found", 0);
            Print("-- TERMINATING Script");
            return;
        }
        string migrationProjectDirectory = Path.GetDirectoryName(migrationProjectPath);
        //Print($"Migrations Project Directory: {migrationProjectDirectory}", 0);

        // Check if repo contains .tt files.
        if (Directory.GetDirectories(migrationProjectDirectory, "*.tt").Length > 0)
        {
            Print("Found .tt files in Migrations project", 0);
            Print("-- TERMINATING Script");
        }

        // Get solution path.
        string solutionPath = Directory.GetFiles(currentDir, "*.sln").FirstOrDefault();
        //Print($"Solution Path: {solutionPath}", 0);
        if (solutionPath == null)
        {
            Print("ERROR: Solution file not found", 0);
            Print("-- TERMINATING Script");
            return;
        }

        // Start processing commits.
        using (var repo = new Repository(currentDir))
        {
            var oldCommit = repo.Lookup<Commit>(oldRef);
            var newCommit = repo.Lookup<Commit>(newRef);
            string oldBranchName = GetBranchNameFromCommit(repo, oldCommit);
            string newBranchName = GetBranchNameFromCommit(repo, newCommit);

            // Display branch names.
            Print($"Coming from '{oldBranchName ?? oldRef}' to '{newBranchName ?? oldRef}'", 0);

            // Search for migrations in the old branch.
            var swFindOld = Stopwatch.StartNew();
            List<string> oldBranchMigrations = FindMigrationsInCommitDir(repo, oldCommit, migrationDir);
            swFindOld.Stop();

            var swFindNew = Stopwatch.StartNew();
            List<string> newBranchMigrations = FindMigrationsInCommitDir(repo, newCommit, migrationDir);
            swFindNew.Stop();

            // Check if there are new/unique migrations in old and new branches.
            List<string> oldBranchMigrationsUnique = oldBranchMigrations.Except(newBranchMigrations).ToList();
            List<string> newBranchMigrationsUnique = newBranchMigrations.Except(oldBranchMigrations).ToList();

            // Check if there are migrations to apply (need to do in old branch).
            Print($"Found {oldBranchMigrationsUnique.Count} old migrations from a total of {oldBranchMigrations.Count} in '{oldBranchName}' ({swFindOld.ElapsedMilliseconds}ms)", 1);
            foreach (var migration in oldBranchMigrationsUnique)
                Print($"{migration}", 2);
            if (oldBranchMigrationsUnique.Count > 0)
            {
                // Check if the branch names are found (so we can navigate to them in order to apply migrations).
                if (oldBranchName == null || newBranchName == null)
                {
                    PrintEmpty();
                    Print("ERROR: Branch names not found", 0);
                    sw.Stop();
                    Print($"-- TERMINATING Script ({sw.ElapsedMilliseconds}ms)", 0);
                    return;
                }

                // Check if has pending changes.
                if (repo.RetrieveStatus().IsDirty)
                {
                    Print("ERROR: Repository has pending changes", 0);
                    sw.Stop();
                    Print($"-- TERMINATING Script ({sw.ElapsedMilliseconds}ms)", 0);
                    return;
                }

                // Check if dotnet-ef is installed.
                if (!(await DotnetHelper.CheckDotnetEFInstalled()))
                {
                    Print("ERROR: dotnet-ef not installed (Please run: `dotnet tool install --global dotnet-ef`)", 0);
                    sw.Stop();
                    Print($"-- TERMINATING Script ({sw.ElapsedMilliseconds}ms)", 0);
                    return;
                }

                // Get all migrations from new branch.
                var newBranchCurrentMigrations = MigratorHelper.FindMigrationsInDir(migrationDir);
                try
                {
                    // Skip the git hook to avoid entering an infinite loop.
                    Environment.SetEnvironmentVariable("BRIGHT_GITEFCORE_SKIP_GIT_HOOK", "1");

                    // Go back to the old branch.
                    Print($"Going back to old branch '{oldBranchName ?? oldRef}'", 1);
                    Commands.Checkout(repo, oldBranchName);

                    // Get all migrations from old branch.
                    var oldBranchCurrentMigrations = MigratorHelper.FindMigrationsInDir(migrationDir);

                    // Get the latest common migration name between the two branches.
                    var commonMigration = MigratorHelper.GetLatestCommonNameBetweenTwoLists(oldBranchCurrentMigrations, newBranchCurrentMigrations);
                    Print($"Found latest common migration: {commonMigration}", 2);

                    // Apply migrations from the common migration name.
                    var commonMigrationName = Path.GetFileNameWithoutExtension(commonMigration);
                    Print($"Applying migration: {commonMigration}", 2);

                    var swMigration = Stopwatch.StartNew();
                    if (!await DotnetHelper.UpdateDatabaseEFCoreAsync(migrationProjectDirectory, commonMigrationName))
                    {
                        Print("ERROR: UpdateDatabaseEFCoreAsync failed", 0);

                        // Try to go back to the new branch.
                        try
                        {
                            // Wait a bit to avoid conflicts.
                            await Task.Delay(500);

                            // Go back to the new branch.
                            Print($"Trying to come back to new branch '{newBranchName ?? newRef}'", 1);
                            Commands.Checkout(repo, newBranchName);

                            Print("Sorry, you'll have to migrate/update DB manually", 0);
                        }
                        catch (Exception ex)
                        {
                            Print($"ERROR: {ex.Message}", 0);
                        }
                        finally
                        {
                            // Remove the skip git hook.
                            Environment.SetEnvironmentVariable("BRIGHT_GITEFCORE_SKIP_GIT_HOOK", "0");
                        }

                        sw.Stop();
                        Print($"-- TERMINATING Script ({sw.ElapsedMilliseconds}ms)", 0);
                        return;
                    }
                    swMigration.Stop();
                    Print($"Update Database applied in {swMigration.ElapsedMilliseconds}ms", 2);

                    // Wait a bit to avoid conflicts.
                    await Task.Delay(500);

                    // Go back to the new branch.
                    Print($"Coming back to new branch '{newBranchName ?? newRef}'", 1);
                    Commands.Checkout(repo, newBranchName);
                }
                catch (Exception ex)
                {
                    Print($"ERROR: {ex.Message}", 0);
                    return;
                }
                finally
                {
                    // Remove the skip git hook.
                    Environment.SetEnvironmentVariable("BRIGHT_GITEFCORE_SKIP_GIT_HOOK", "0");
                }
            }

            // TODO: THIS IS WORKING FINE!
            // Still, we might not actually want to run the Up migrations (probably a setting default to false?).
            // They will run automatically when the application starts.

            // Apply Up migrations.
            Print($"Found {newBranchMigrationsUnique.Count} new migrations from a total of {newBranchMigrations.Count} in '{newBranchName}' ({swFindNew.ElapsedMilliseconds}ms)", 1);
            foreach (var migration in newBranchMigrationsUnique)
                Print($"{migration}", 2);

            if (newBranchMigrationsUnique.Count > 0)
            {
                bool runUpMigrations = false;
                if (runUpMigrations)
                {
                    // Check if dotnet-ef is installed.
                    if (!(await DotnetHelper.CheckDotnetEFInstalled()))
                    {
                        Print("ERROR: dotnet-ef not installed (Please run: `dotnet tool install --global dotnet-ef`)", 0);
                        sw.Stop();
                        Print($"-- TERMINATING Script ({sw.ElapsedMilliseconds}ms)", 0);
                        return;
                    }

                    Print($"Applying 'update database'", 2);
                    var swMigration = Stopwatch.StartNew();
                    if (!await DotnetHelper.UpdateDatabaseEFCoreAsync(migrationProjectDirectory, string.Empty))
                    {
                        Print("ERROR: UpdateDatabaseEFCoreAsync failed", 0);
                        sw.Stop();
                        Print($"-- TERMINATING Script ({sw.ElapsedMilliseconds}ms)", 0);
                        return;
                    }
                    swMigration.Stop();
                    Print($"Update Database applied in {swMigration.ElapsedMilliseconds}ms", 2);
                }
                else
                {
                    Print("Skipping 'update database' for Up migrations (just run the application)", 1);
                }
            }
        }

        Print($"-- Bright Git-EFCore: Finished ({sw.ElapsedMilliseconds}ms)");
    }

    static string GetBranchNameFromCommit(Repository repo, Commit commit)
    {
        var branches = repo.Branches.Where(b => b.Tip.Sha == commit.Sha).ToList();
        return branches.FirstOrDefault()?.FriendlyName;
    }

    static List<string> FindMigrationsInCommit(Repository repo, Commit commit, string migrationDir)
    {
        var migrationFiles = new List<string>();

        // Remove the repo directory from the migrationDir and replace the backslashes with forward slashes.
        migrationDir = migrationDir.Replace(repo.Info.WorkingDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);
        migrationDir = migrationDir.Replace(Path.DirectorySeparatorChar, '/');

        // Filter commit changes by migration path and .cs files.
        var changes = commit.Parents.SelectMany(parent => repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree))
                    .Where(change => change.Path.StartsWith(migrationDir, StringComparison.OrdinalIgnoreCase) &&
                                     change.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                                     !change.Path.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(change => change.Path)
                    .ToList();

        // Check only for .cs files containing this specific date format (a real migration).
        string dateformat = "yyyyMMddHHmmss";
        changes.RemoveAll(change => !DateTime.TryParseExact(Path.GetFileName(change.Path)[..14], dateformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));

        //Print($"Found {changes.Count} migrations in commit", 2);
        foreach (var change in changes)
        {
            var fileBlob = commit[change.Path]?.Target as Blob;
            if (fileBlob != null)
            {
                Print($"{change.Path}", 3);
                migrationFiles.Add(change.Path);
            }
        }

        return migrationFiles;
    }

    static List<string> FindMigrationsInCommitDir(Repository repo, Commit commit, string migrationDir)
    {
        // Remove the repo directory from the migrationDir and replace the backslashes with forward slashes.
        migrationDir = migrationDir.Replace(repo.Info.WorkingDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);
        migrationDir = migrationDir.Replace(Path.DirectorySeparatorChar, '/');

        // Get the tree associated with the commit.
        var tree = commit.Tree;

        // Find the folder within the tree.
        var folderEntry = tree[migrationDir];
        if (folderEntry == null || folderEntry.TargetType != TreeEntryTargetType.Tree)
        {
            return null;
        }

        // Get the tree representing the folder.
        var folderTree = (Tree)folderEntry.Target;

        // List all files in the folder.
        var migrations = folderTree
            .Where(entry => entry.TargetType == TreeEntryTargetType.Blob)
            .Select(entry => entry.Path)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                           !path.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p)
            .ToList();

        // Check only for .cs files containing this specific date format (a real migration).
        string dateformat = "yyyyMMddHHmmss";
        migrations.RemoveAll(migration => !DateTime.TryParseExact(Path.GetFileName(migration)[..14], dateformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));

        return migrations;
    }

    #region Log/Console

    static void Print(string msg, int indentationLevel = 0)
    {
        // Writes to Console.WriteLine with indentation.
        Console.WriteLine($"{new string(' ', indentationLevel * 3)}{msg}");
    }

    static void PrintEmpty()
    {
        Console.WriteLine();
    }

    #endregion
}
