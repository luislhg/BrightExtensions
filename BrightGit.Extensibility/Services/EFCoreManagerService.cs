using BrightGit.SharpCommon;
using BrightGit.SharpCommon.Helpers;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.ProgressReporting;
using System.Diagnostics;

namespace BrightGit.Extensibility.Services;

public class EFCoreManagerService
{
    // TODO: Maybe this should be injected and then EFCoreManagerService would need to be Scoped (accordingly to samples/doc in VS API).
    public VisualStudioExtensibility Extensibility { get; set; }

    // True while a migration check/update is running (GitFileWatcherService uses it to ignore HEAD changes during it).
    public bool IsBusy { get; private set; }

    // Simple toggle: show a progress dialog while reverting migrations (true) or only the VS status bar progress report (false).
    public bool ShowProgressDialog { get; set; } = true;

    private readonly TraceSource logger;
    private readonly SettingsService settingsService;
    private readonly IDialogService dialogService;

    public EFCoreManagerService(TraceSource logger, SettingsService settingsService, IDialogService dialogService)
    {
        this.logger = logger;
        this.settingsService = settingsService;
        this.dialogService = dialogService;
    }

    public async Task<bool> CheckMigrationsAsync(string solutionDir, string oldBranchName, string currentBranchName)
    {
        //if (!settingsService.Data.EFCore.IsEnabled || IsBusy)
        if (IsBusy)
            return false;

        var sw = Stopwatch.StartNew();
        var shell = Extensibility.Shell();

        try
        {
            IsBusy = true;

            // Check if the solution directory is a git repository.
            if (solutionDir == null || !Directory.GetDirectories(solutionDir, ".git").Any())
            {
                logger.TraceEvent(TraceEventType.Warning, 0, "Repository directory not found.");
                return false;
            }

            // Get migrations directory.
            string migrationDir = MigratorHelper.GetMigrationsDirectory(solutionDir);
            if (migrationDir == null)
            {
                logger.TraceInformation("Migrations directory not found, skipping EF Core migrations check.");
                return false;
            }

            // Compare migrations between the branch we left and the branch we are on now.
            var oldBranchMigrations = FindDBMigrationsInBranchSafe(solutionDir, oldBranchName);
            var currentBranchMigrations = FindDBMigrationsInBranchSafe(solutionDir, currentBranchName);
            var oldBranchMigrationsUnique = oldBranchMigrations.Except(currentBranchMigrations).ToList();
            var currentBranchMigrationsUnique = currentBranchMigrations.Except(oldBranchMigrations).ToList();
            sw.Stop();

            logger.TraceInformation($"Found {oldBranchMigrationsUnique.Count} migrations to revert from '{oldBranchName}' and {currentBranchMigrationsUnique.Count} new in '{currentBranchName}' ({sw.ElapsedMilliseconds}ms).");
            Debug.WriteLine($"Found {oldBranchMigrationsUnique.Count} migrations to revert from '{oldBranchName}' and {currentBranchMigrationsUnique.Count} new in '{currentBranchName}' ({sw.ElapsedMilliseconds}ms).");

            // Migrations that only exist in the old branch must be reverted (Down) while their code is still available there.
            if (oldBranchMigrationsUnique.Count > 0)
            {
                // Check if dotnet-ef is installed.
                if (!await DotnetHelper.CheckDotnetEFInstalled())
                {
                    await shell.ShowPromptAsync("dotnet-ef is not installed (Please run: 'dotnet tool install --global dotnet-ef').", PromptOptions.OK, CancellationToken.None);
                    return false;
                }

                // Ask the user before touching the database.
                var resultApply = await shell.ShowPromptAsync($"'{oldBranchName}' has {oldBranchMigrationsUnique.Count} migrations not present in '{currentBranchName}'.\n\nUpdate database down to the latest common migration?\n(This runs on a temporary copy of '{oldBranchName}', your working directory is not touched)", PromptOptions.OKCancel, CancellationToken.None);
                if (!resultApply)
                    return false;

                // Get all migrations currently on disk (current branch).
                var currentBranchDiskMigrations = MigratorHelper.FindMigrationsInDir(migrationDir);

                // Checking out the old branch in place would fight with VS over locked files (project reloads, test discovery, etc.)
                // and a failed checkout leaves the working directory full of pending changes, so we use a temporary worktree instead.
                string worktreePath = Path.Combine(Path.GetTempPath(), "BrightGit", $"Worktree_{Guid.NewGuid().ToString("N")[..8]}");
                Directory.CreateDirectory(Path.GetDirectoryName(worktreePath));

                // Report progress through a dialog or the VS status bar (restore + build + database update can take quite a few seconds).
                Action<int, string> reportProgress;
                Func<Task> completeProgressAsync;
                if (ShowProgressDialog)
                {
                    dialogService.Shell = shell;
                    var dialogTask = dialogService.ShowDialogProgressAsync($"Reverting EF Core migrations from '{oldBranchName}'", out var updateDialog, CancellationToken.None);
                    reportProgress = (percent, message) => updateDialog(percent, message, false);
                    completeProgressAsync = async () =>
                    {
                        try
                        {
                            // Complete and close the dialog programmatically.
                            updateDialog(100, null, true);
#pragma warning disable VSTHRD003 // Out-of-proc RPC task (no JTF context), awaiting it here cannot deadlock.
                            await dialogTask;
#pragma warning restore VSTHRD003
                        }
                        catch (Exception ex)
                        {
                            // The dialog may have been closed by the user already, never let it break the flow.
                            Debug.WriteLine(ex.Message);
                        }
                    };
                }
                else
                {
                    var progressReporter = await shell.StartProgressReportingAsync("Reverting EF Core migrations", new(false), CancellationToken.None);
                    reportProgress = (percent, message) => progressReporter.Report(new ProgressStatus(percent, message));
                    completeProgressAsync = () => { progressReporter.Dispose(); return Task.CompletedTask; };
                }

                string errorMessage, commonMigrationName;
                sw.Restart();
                try
                {
                    (errorMessage, commonMigrationName) = await RevertMigrationsInWorktreeAsync(solutionDir, oldBranchName, worktreePath, currentBranchDiskMigrations, reportProgress);
                }
                finally
                {
                    // Remove the temporary worktree (best effort) and close the progress.
                    reportProgress(90, "Removing temporary copy");
                    await RemoveWorktreeSafeAsync(solutionDir, worktreePath);
                    await completeProgressAsync();
                }
                sw.Stop();

                // Inform the user about the result (only after the progress is closed).
                if (errorMessage != null)
                {
                    await shell.ShowPromptAsync(errorMessage, PromptOptions.OK, CancellationToken.None);
                    return false;
                }

                await shell.ShowPromptAsync($"Database updated down to common migration '{commonMigrationName}' ({sw.ElapsedMilliseconds}ms).", PromptOptions.OK, CancellationToken.None);
            }

            // Migrations that only exist in the current branch (Up) are applied when the application runs, so just log them.
            if (currentBranchMigrationsUnique.Count > 0)
                logger.TraceInformation($"Skipping 'update database' for {currentBranchMigrationsUnique.Count} new migrations in '{currentBranchName}' (just run the application).");

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<(string Error, string CommonMigrationName)> RevertMigrationsInWorktreeAsync(string solutionDir,
                                                                                                   string oldBranchName,
                                                                                                   string worktreePath,
                                                                                                   List<string> currentBranchDiskMigrations,
                                                                                                   Action<int, string> reportProgress)
    {
        var sw = new Stopwatch();

        // Create the temporary worktree of the old branch.
        logger.TraceInformation($"Creating temporary worktree of '{oldBranchName}' at '{worktreePath}'.");
        reportProgress(5, $"Creating temporary copy of '{oldBranchName}'");
        if (!await GitHelper.AddWorktreeAsync(solutionDir, worktreePath, oldBranchName))
        {
            logger.TraceEvent(TraceEventType.Error, 0, "Failed to create temporary worktree.");
            return ("Failed to create a temporary git worktree (is git available on PATH?).\nYou'll have to update the database manually.", null);
        }

        // Find the migrations directory and project inside the worktree (old branch content).
        string worktreeMigrationDir = MigratorHelper.GetMigrationsDirectory(worktreePath);
        string worktreeProjectPath = worktreeMigrationDir != null ? MigratorHelper.GetProjectFilePathFromInsideOut(worktreeMigrationDir) : null;
        if (worktreeProjectPath == null)
        {
            logger.TraceEvent(TraceEventType.Error, 0, "Migrations project not found in the worktree.");
            return ("Migrations project not found in the temporary worktree.\nYou'll have to update the database manually.", null);
        }
        string worktreeProjectDirectory = Path.GetDirectoryName(worktreeProjectPath);

        // Restore NuGet packages (the worktree starts with no bin/obj and 'dotnet ef' builds without restoring).
        logger.TraceInformation("Restoring NuGet packages in the worktree.");
        reportProgress(20, "Restoring NuGet packages");
        sw.Restart();
        if (!await DotnetHelper.RestoreProjectAsync(worktreeProjectDirectory))
        {
            logger.TraceEvent(TraceEventType.Error, 0, "Failed to restore NuGet packages in the worktree.");
            return ("Error restoring NuGet packages for the migrations project.\nYou'll have to update the database manually.", null);
        }
        sw.Stop();
        logger.TraceInformation($"NuGet packages restored ({sw.ElapsedMilliseconds}ms).");
        Debug.WriteLine($"NuGet packages restored ({sw.ElapsedMilliseconds}ms).");

        // Get the latest common migration between the two branches (compare by file name since the lists come from different directories).
        var oldBranchDiskMigrations = MigratorHelper.FindMigrationsInDir(worktreeMigrationDir);
        var commonMigration = MigratorHelper.GetLatestCommonNameBetweenTwoLists(oldBranchDiskMigrations.Select(Path.GetFileName).ToList(),
                                                                                currentBranchDiskMigrations.Select(Path.GetFileName).ToList());

        // "0" reverts all migrations (when the branches have none in common).
        var commonMigrationName = commonMigration != null ? Path.GetFileNameWithoutExtension(commonMigration) : "0";

        // Update the database (builds the project first, this is the longest step).
        logger.TraceInformation($"Updating database down to migration '{commonMigrationName}'.");
        reportProgress(40, $"Updating database down to '{commonMigrationName}'");
        sw.Restart();
        if (!await DotnetHelper.UpdateDatabaseEFCoreAsync(worktreeProjectDirectory, commonMigrationName))
        {
            logger.TraceEvent(TraceEventType.Error, 0, "UpdateDatabaseEFCoreAsync failed.");
            return ("Error updating database, you'll have to update it manually.", commonMigrationName);
        }
        sw.Stop();
        logger.TraceInformation($"Database updated down to '{commonMigrationName}' ({sw.ElapsedMilliseconds}ms).");
        Debug.WriteLine($"Database updated down to '{commonMigrationName}' ({sw.ElapsedMilliseconds}ms).");

        return (null, commonMigrationName);
    }

    private List<string> FindDBMigrationsInBranchSafe(string repoDir, string branchName)
    {
        try
        {
            return GitHelper.FindDBMigrationsInBranch(repoDir, branchName);
        }
        catch (Exception ex)
        {
            // The branch (or its Migrations folder) may not exist.
            logger.TraceEvent(TraceEventType.Warning, 0, $"Could not read migrations from branch '{branchName}': {ex.Message}");
            return new List<string>();
        }
    }

    private async Task RemoveWorktreeSafeAsync(string repoDir, string worktreePath)
    {
        try
        {
            if (Directory.Exists(worktreePath))
            {
                // Fallback to deleting the folder and pruning the metadata if git refuses.
                if (!await GitHelper.RemoveWorktreeAsync(repoDir, worktreePath))
                {
                    Directory.Delete(worktreePath, true);
                    await GitHelper.PruneWorktreesAsync(repoDir);
                }
            }
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Warning, 0, $"Failed to remove temporary worktree '{worktreePath}': {ex.Message}");
        }
    }
}
