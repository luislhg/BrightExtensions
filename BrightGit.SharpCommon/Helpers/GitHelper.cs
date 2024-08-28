using LibGit2Sharp;
using System.Globalization;

namespace BrightGit.SharpCommon.Helpers;
public static class GitHelper
{
    public static List<string> FindDBMigrationsInCurrentBranch(string repoDir)
    {
        using (var repo = new Repository(repoDir))
        {
            // Get the current branch.
            var currentBranch = repo.Branches.FirstOrDefault(b => b.IsCurrentRepositoryHead);
            if (currentBranch == null)
            {
                throw new InvalidOperationException("No current branch found.");
            }

            // Get the tip of the current branch.
            var currentCommit = currentBranch.Tip;
            if (currentCommit == null)
            {
                throw new InvalidOperationException($"No commit found in branch {currentBranch.FriendlyName}.");
            }

            // Define the migrations directory path relative to the repository root.
            var migrationDir = GetMigrationsDirectory(repoDir);
            var relativeMigrationDir = migrationDir.Replace(repo.Info.WorkingDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);

            // Get migration files from the current branch.
            return GetMigrationFilesFromTree(currentCommit.Tree, relativeMigrationDir);
        }
    }

    public static List<string> FindDBMigrationsInCommonAncestor(string repoDir, string branchParent)
    {
        using (var repo = new Repository(repoDir))
        {
            // Get the current branch.
            var currentBranch = repo.Branches.FirstOrDefault(b => b.IsCurrentRepositoryHead);
            if (currentBranch == null)
            {
                throw new InvalidOperationException("No current branch found.");
            }

            // Get the base branch.
            var baseBranch = repo.Branches[branchParent];
            if (baseBranch == null)
            {
                throw new InvalidOperationException($"Base branch {branchParent} not found.");
            }

            // Find the common ancestor.
            var commonAncestor = repo.ObjectDatabase.FindMergeBase(currentBranch.Tip, baseBranch.Tip);
            if (commonAncestor == null)
            {
                throw new InvalidOperationException($"No common ancestor found between current branch and {branchParent}.");
            }

            // Define the migrations directory path relative to the repository root.
            var migrationDir = GetMigrationsDirectory(repoDir);
            var relativeMigrationDir = migrationDir.Replace(repo.Info.WorkingDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);

            // Get migration files from the common ancestor.
            return GetMigrationFilesFromTree(commonAncestor.Tree, relativeMigrationDir);
        }
    }

    private static List<string> GetMigrationFilesFromTree(Tree tree, string folderPath)
    {
        // Replace backslashes with forward slashes (git uses /).
        folderPath = folderPath.Replace("\\", "/");

        var migrations = new List<string>();
        var folder = tree[folderPath];
        if (folder == null || folder.TargetType != TreeEntryTargetType.Tree)
        {
            throw new InvalidOperationException($"Folder {folderPath} not found in the tree.");
        }

        var folderTree = (Tree)folder.Target;
        foreach (var entry in folderTree)
        {
            if (entry.TargetType == TreeEntryTargetType.Blob)
            {
                if (entry.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    !entry.Name.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase))
                {
                    // Add the migration file path with the correct path separator.
                    migrations.Add(entry.Path.Replace("/", "\\"));
                }
            }
        }

        // Check only for .cs files containing this specific date format (a real migration).
        string dateformat = "yyyyMMddHHmmss";
        migrations.RemoveAll(file => !DateTime.TryParseExact(Path.GetFileName(file)[..14], dateformat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));

        return migrations;
    }

    public static string FindBranchStartingCommit(string repoDir, string branchName)
    {
        using (var repo = new Repository(repoDir))
        {
            var branch = repo.Branches[branchName];
            if (branch == null)
            {
                throw new InvalidOperationException($"Branch {branchName} not found.");
            }

            // Find the remote tracking branch.
            var remoteTrackingBranch = branch.TrackedBranch;
            if (remoteTrackingBranch == null)
            {
                throw new InvalidOperationException($"No remote tracking branch found for {branchName}.");
            }

            // Find the common ancestor between the current branch and its remote tracking branch.
            var commonAncestor = repo.ObjectDatabase.FindMergeBase(branch.Tip, remoteTrackingBranch.Tip);
            if (commonAncestor == null)
            {
                throw new InvalidOperationException($"No common ancestor found between {branchName} and its remote tracking branch {remoteTrackingBranch.FriendlyName}.");
            }

            return commonAncestor.Sha;
        }
    }

    public static List<string> FindDBMigrationsInCurrentBranchCommits(string repoDir)
    {
        var migrationFiles = new List<string>();

        // Open the repository
        using (var repo = new Repository(repoDir))
        {
            // Get the current branch
            var currentBranch = repo.Branches.FirstOrDefault(b => b.IsCurrentRepositoryHead);
            if (currentBranch == null)
            {
                throw new InvalidOperationException("No current branch found.");
            }

            // Iterate over the commits in the current branch
            foreach (var commit in currentBranch.Commits)
            {
                // Find migrations in the commit
                migrationFiles.AddRange(FindMigrationsInCommit(repo, commit, GetMigrationsDirectory(repoDir)));
            }
        }

        return migrationFiles;
    }

    public static string GetCurrentBranchName(string repoDir)
    {
        using (var repo = new Repository(repoDir))
        {
            // Get the current branch.
            var currentBranch = repo.Branches.FirstOrDefault(b => b.IsCurrentRepositoryHead);
            if (currentBranch == null)
            {
                throw new InvalidOperationException("No current branch found.");
            }

            return currentBranch.FriendlyName;
        }
    }

    private static List<string> FindMigrationsInCommit(Repository repo, Commit commit, string migrationDir)
    {
        var migrationFiles = new List<string>();

        if (migrationDir == null)
        {
            return migrationFiles;
        }

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

        foreach (var change in changes)
        {
            var fileBlob = commit[change.Path]?.Target as Blob;
            if (fileBlob != null)
            {
                migrationFiles.Add(change.Path);
            }
        }

        return migrationFiles;
    }

    private static string GetMigrationsDirectory(string repoDir)
    {
        // Check if the repoDir exists.
        if (!Directory.Exists(repoDir))
            return null;

        // Search for the Migrations directory.
        var directories = Directory.GetDirectories(repoDir, "Migrations", SearchOption.AllDirectories);

        // Return the first found Migrations directory, or null if not found.
        return directories.Length > 0 ? directories[0] : null;
    }

    public static List<string> GetRootBranches(string repoDir)
    {
        // Check if the repoDir exists.
        if (!Directory.Exists(repoDir))
            return null;

        // Get all branches in the repository that are not tracking branches and do not have separators.
        using (var repo = new Repository(repoDir))
        {
            return repo.Branches.Where(b => !b.IsRemote && !b.FriendlyName.Contains("/")).Select(b => b.FriendlyName).ToList();
        }
    }

    public static string GetBranchNameFromCommit(Repository repo, Commit commit)
    {
        var branches = repo.Branches.Where(b => b.Tip.Sha == commit.Sha).ToList();
        return branches.FirstOrDefault()?.FriendlyName;
    }
}
