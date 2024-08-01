using BrightGit.SharpCommon;
using BrightGit.SharpCommon.Helpers;
using BrightGit.SharpCommon.Models;
using LibGit2Sharp;
using System.Diagnostics;

namespace BrightGit.SharpRun;
public class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("ERROR: Specify action and repo dir.");
            return;
        }

        RunData runData = new RunData
        {
            RunType = Enum.Parse<RunType>(args[0]),
            RepoDir = args[1],
            Parameters = args.Skip(2).ToArray()
        };

        switch (runData.RunType)
        {
            case RunType.EFMigrationUp:
                EFMigrationUp(runData.RepoDir);
                break;
            case RunType.EFMigrationDown:
                EFMigrationDown(runData.RepoDir);
                break;

            // TODO: These are kind of a placeholder, not really implemented.
            //case RunType.GitUndoChanges:
            //    GitUndoChanges(runData.RepoDir);
            //    break;
            //case RunType.GitStashChanges:
            //    GitStashChanges(runData.RepoDir);
            //    break;
            //case RunType.GitStashPop:
            //    GitStashPop(runData.RepoDir);
            //    break;
            //case RunType.GitStashApply:
            //    GitStashApply(runData.RepoDir);
            //    break;
            default:
                Console.WriteLine($"ERROR: Unknown action: {runData.RunType}");
                break;
        }
    }

    static void EFMigrationDown(string repoDir)
    {
        var sw = Stopwatch.StartNew();

        if (!DotnetHelper.CheckDotnetEFInstalled().Result)
        {
            PrintError("dotnet-ef tool is not installed.");
            return;
        }
        try
        {
            // Get all migrations in the current branch.
            var dbMigrations = GitHelper.FindDBMigrationsInCurrentBranch(repoDir);
            PrintInfo($"Found {dbMigrations.Count} migrations in the current branch ({sw.ElapsedMilliseconds}).");
            sw.Stop();

            // Apply the migrations.
            if (dbMigrations.Any())
            {
                var migrationFiles = string.Join("\n", dbMigrations);
                PrintInfo($"The following migrations will be applied:{Environment.NewLine}{migrationFiles}");

                // Apply the migrations.
                PrintLog("Applying migrations...");
                PrintLog("Migrations applied.");
            }
            else
            {
                PrintInfo("No migrations found in the current branch.");
            }
        }
        catch (Exception ex)
        {
            PrintError($"Error applying migrations: {ex.Message}");
        }
    }

    // TODO: Usually we don't migrate up, but this is just a placeholder.
    static void EFMigrationUp(string repoDir)
    {
        if (!DotnetHelper.CheckDotnetEFInstalled().Result)
        {
            Console.WriteLine("dotnet-ef tool is not installed.");
            return;
        }

        DotnetHelper.BuildSolutionEFCore(repoDir);
        Console.WriteLine("EF migrations applied.");
    }

    static void PrintError(string message)
    {
        Console.WriteLine($"ERROR: {message}");
    }

    static void PrintInfo(string message)
    {
        Console.WriteLine($"INFO: {message}");
    }

    static void PrintLog(string message)
    {
        Console.WriteLine($"LOG: {message}");
    }

    #region TODO / Future

    static void GitUndoChanges(string repoDir)
    {
        using (var repo = new Repository(repoDir))
        {
            foreach (var file in repo.RetrieveStatus().Where(status => status.State != FileStatus.Unaltered))
            {
                Commands.Unstage(repo, file.FilePath);
                repo.CheckoutPaths(repo.Head.FriendlyName, new[] { file.FilePath });
            }
        }
        Console.WriteLine("Changes undone.");
    }

    static void GitStashChanges(string repoDir)
    {
        using (var repo = new Repository(repoDir))
        {
            repo.Stashes.Add(new Signature("BrightGit", string.Empty, DateTime.Now), "Stash created by BrightGit");
        }
        Console.WriteLine("Changes stashed.");
    }

    static void GitStashPop(string repoDir)
    {
        using (var repo = new Repository(repoDir))
        {
            repo.Stashes.Pop(0);
        }
        Console.WriteLine("Stash popped.");
    }

    static void GitStashApply(string repoDir)
    {
        using (var repo = new Repository(repoDir))
        {
            repo.Stashes.Apply(0);
        }
        Console.WriteLine("Stash applied.");
    }

    #endregion
}