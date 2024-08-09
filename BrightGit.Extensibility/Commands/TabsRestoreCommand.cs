namespace BrightGit.Extensibility.Commands;

using BrightGit.Extensibility.Helpers;
using BrightGit.Extensibility.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.OpenDocument;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class TabsRestoreCommand : Command
{
    private readonly TraceSource logger;
    private readonly TabsStorageService tabsStorageService;

    public TabsRestoreCommand(TraceSource traceSource, TabsStorageService tabsStorageService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.tabsStorageService = tabsStorageService;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "(WIP) Restore Tabs")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.RestoreDefaultView, IconSettings.IconAndText),
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
                gitBranchName = File.ReadAllText(gitHeadPath).Split('/').LastOrDefault();
                gitBranchName = $"_{gitBranchName}";
            }

            // Read from disk.
            string tabsSaveKey = $"TabsCustom_{solutionName}{gitBranchName}";
            Debug.WriteLine(tabsSaveKey);

            // Check if there are any tabs to restore.
            if (!string.IsNullOrEmpty(tabsSaveKey))
            {
                // Deserialize.
                var tabsInfo = tabsStorageService.Data.TabsCustom.FirstOrDefault(t => t.Id == tabsSaveKey);

                // TODO: No longer intersecting because .xaml files (MAUI projet) are not being closed properly.
                // We close via the API, VS closes it, but GetOpenDocuments is still listing it.

                // Intersect opened documents with saved documents (spare processing time and inform the user about re-restoring something already opened).
                //var openedDocuments = await documents.GetOpenDocumentsAsync(cancellationToken);
                //var tabDocuments = tabsInfo.Tabs.Where(sd => !openedDocuments.Any(od => od.Moniker.LocalPath == sd.FilePath)).ToList();
                var tabDocuments = tabsInfo.Tabs;

                // Check if there are any documents to restore.
                if (tabDocuments.Count == 0)
                {
                    await shell.ShowPromptAsync("No tabs to restore", PromptOptions.OK, cancellationToken);
                    return;
                }

                // TODO: In the future find some way to open documents faster, without activating/viewing one by one.

                // Open documents.
                var openDocumentOptions = new OpenDocumentOptions(activate: false);
                await Task.WhenAll(tabDocuments.Select(document => Extensibility.Documents().OpenDocumentAsync(new Uri(document.FilePath), openDocumentOptions, cancellationToken)));

                // TODO: We need some way to pin documents again (once we figure out how to do it).

                sw.Stop();
                Debug.WriteLine($"Restored {tabDocuments.Count} tabs for {solutionName} ({sw.ElapsedMilliseconds}ms)");
                await shell.ShowPromptAsync($"Restored {tabDocuments.Count} tabs for {solutionName} ({sw.ElapsedMilliseconds}ms)", PromptOptions.OK, cancellationToken);
            }
            else
            {
                await shell.ShowPromptAsync("No tabs saved for this solution/branch", PromptOptions.OK, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await shell.ShowPromptAsync($"Error restoring tabs: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }
}
