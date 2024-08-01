namespace BrightGit.Extensibility.Commands;

using BrightGit.Extensibility.Helpers;
using BrightGit.Extensibility.Services;
using BrightGit.SharpCommon;
using BrightGit.SharpCommon.Helpers;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class EFGitMigrationDownCommand : Command
{
    private readonly TraceSource logger;
    private readonly IDialogService dialogService;

    public EFGitMigrationDownCommand(TraceSource traceSource, IDialogService dialogService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.dialogService = dialogService;
        dialogService.Shell = Extensibility.Shell();
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "EF Core - Reset Branch Migrations from Database")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.LinkedDatabase, IconSettings.IconAndText),
    };

    /// <inheritdoc />
    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Use InitializeAsync for any one-time setup or initialization.
        return base.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var shell = Extensibility.Shell();
        var workspace = Extensibility.Workspaces();
        var documents = Extensibility.Documents();

        // Find the solution directory.
        var solutionPath = await VSHelper.GetSolutionPathAsync(workspace, cancellationToken);
        if (solutionPath == null)
        {
            logger.TraceEvent(TraceEventType.Error, 0, "Solution path not found.");
            await shell.ShowPromptAsync("Please open a solution first.", PromptOptions.OK, cancellationToken);
            return;
        }

        // Find the repository directory.
        var repoDir = Path.GetDirectoryName(solutionPath);
        if (repoDir == null || !Directory.GetDirectories(repoDir).Any(p => p.EndsWith(".git")))
        {
            logger.TraceEvent(TraceEventType.Error, 0, "Repository directory not found.");
            await shell.ShowPromptAsync("Repository directory not found.", PromptOptions.OK, cancellationToken);
            return;
        }

        // Get migrations directory.
        string migrationDir = GetMigrationsDirectory(repoDir);
        if (migrationDir == null)
        {
            logger.TraceEvent(TraceEventType.Error, 0, "Migrations directory not found.");
            await shell.ShowPromptAsync("Migrations directory not found.", PromptOptions.OK, cancellationToken);
            return;
        }

        // Get migrations project file and directory.
        string migrationProjectPath = GetProjectFilePathFromInsideOut(migrationDir);
        if (migrationProjectPath == null)
        {
            logger.TraceEvent(TraceEventType.Error, 0, "Project file not found.");
            await shell.ShowPromptAsync("Project file not found.", PromptOptions.OK, cancellationToken);
            return;
        }
        string migrationProjectDirectory = Path.GetDirectoryName(migrationProjectPath);

        try
        {
            // Get the current branch name.
            var currentBranchName = GitHelper.GetCurrentBranchName(repoDir);

            // Grab main branch options (minus our own if it's listed).
            var rootBranchOptions = GitHelper.GetRootBranches(repoDir);
            rootBranchOptions.Remove(currentBranchName);

            // Ask the user to select the branch to compare against.
            var resultBranch = await dialogService.ShowPromptOptionsAsync($"Select the branch to compare against current '{currentBranchName}'", rootBranchOptions, cancellationToken);
            if (resultBranch != null)
            {
                // Get all migrations in the current branch.
                var currentDbMigrations = GitHelper.FindDBMigrationsInCurrentBranch(repoDir);
                var parentDbMigrations = GitHelper.FindDBMigrationsInCommonAncestor(repoDir, resultBranch);
                var currentNewDbMigrations = currentDbMigrations.Except(parentDbMigrations).ToList();
                sw.Stop();

                var msg = $"Found {currentNewDbMigrations.Count} new migrations in '{currentBranchName}' ({sw.ElapsedMilliseconds:N0}ms).";
                Debug.WriteLine(msg);

                // Apply the migrations.
                if (currentNewDbMigrations.Any())
                {
                    var migrationFiles = string.Join("\n", currentNewDbMigrations);
                    Debug.WriteLine($"Migrations found:\n{migrationFiles}");

                    var commonMigration = MigratorHelper.GetLatestCommonNameBetweenTwoLists(currentDbMigrations, parentDbMigrations);
                    var commonMigrationName = Path.GetFileNameWithoutExtension(commonMigration);
                    var message = $"Going back to the latest common migration: {commonMigrationName}";

                    var resultApply = await shell.ShowPromptAsync($"{msg}\n\nUpdate database to last common migration with '{resultBranch}'?\n({commonMigrationName})",
                                                                  PromptOptions.OKCancel,
                                                                  cancellationToken);

                    if (resultApply)
                    {
                        sw.Restart();
                        if (await DotnetHelper.UpdateDatabaseEFCoreAsync(migrationProjectDirectory, commonMigrationName))
                        {
                            sw.Stop();
                            Debug.WriteLine($"Migrations applied ({sw.ElapsedMilliseconds:N0}ms).");
                            await shell.ShowPromptAsync($"Migrations applied ({sw.ElapsedMilliseconds:N0}ms).", PromptOptions.OK, cancellationToken);
                        }
                        else
                        {
                            await shell.ShowPromptAsync("Error applying migrations.", PromptOptions.OK, cancellationToken);
                        }
                    }
                }
                else
                {
                    await shell.ShowPromptAsync($"No new migrations found in current branch when compared to '{resultBranch}'.", PromptOptions.OK, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0, ex.Message);
            await shell.ShowPromptAsync($"Error applying migrations: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }

    #region Helpers

    static string GetMigrationsDirectory(string repoDir)
    {
        // Check if the repoDir exists.
        if (!Directory.Exists(repoDir))
            return null;

        // Search for the Migrations directory.
        var directories = Directory.GetDirectories(repoDir, "Migrations", SearchOption.AllDirectories);

        // Return the first found Migrations directory, or null if not found.
        return directories.Length > 0 ? directories[0] : null;
    }

    static string GetProjectFilePathFromInsideOut(string childrDir)
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

    #endregion
}
