namespace BrightGit.Extensibility.Commands;

using BrightGit.Extensibility.Helpers;
using BrightGit.Extensibility.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class TabsSaveCommand : Command
{
    private readonly TraceSource logger;
    private readonly SettingsService settingsService;
    private readonly TabsStorageService tabsStorageService;
    private readonly TabManagerService tabManagerService;

    public TabsSaveCommand(TraceSource traceSource,
                           SettingsService settingsService,
                           TabsStorageService tabsStorageService,
                           TabManagerService tabManagerService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.settingsService = settingsService;
        this.tabsStorageService = tabsStorageService;
        this.tabManagerService = tabManagerService;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "(WIP) Save Tabs")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.SaveFileDialog, IconSettings.IconAndText),
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
        var shell = Extensibility.Shell();
        var documents = Extensibility.Documents();
        var configuration = Extensibility.Configuration();
        var workspaces = Extensibility.Workspaces();

        try
        {
            var sw = Stopwatch.StartNew();

            // Check if we are in a solution.
            var solutionName = await VSHelper.GetSolutionNameAsync(workspaces, cancellationToken);
            if (string.IsNullOrWhiteSpace(solutionName))
            {
                await shell.ShowPromptAsync("Please open a solution before restoring tabs", PromptOptions.OK, cancellationToken);
                return;
            }

            // Format the branch name.
            string gitHeadPath = Path.Combine(await VSHelper.GetSolutionDirectoryAsync(workspaces, cancellationToken), ".git", "HEAD");
            string gitBranchName = string.Empty;
            if (File.Exists(gitHeadPath))
            {
                gitBranchName = File.ReadAllText(gitHeadPath).Split('/').LastOrDefault().TrimEnd('\n');
                gitBranchName = $"{gitBranchName}";
            }

            var tabsSaved = await tabManagerService.SaveTabsAsync(true, gitBranchName, shell, documents, workspaces, cancellationToken);

            sw.Stop();
            Debug.WriteLine($"Saved tabs {tabsSaved?.Tabs?.Count} for {tabsSaved?.SolutionName}.{gitBranchName} ({sw.ElapsedMilliseconds}ms)");
            await shell.ShowPromptAsync($"Saved {tabsSaved?.Tabs.Count} tabs for {tabsSaved?.SolutionName}.{gitBranchName} ({sw.ElapsedMilliseconds}ms)", PromptOptions.OK, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await shell.ShowPromptAsync($"Error saving tabs: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }
}
