namespace BrightGit.Extensibility.Commands;

using BrightGit.Extensibility.Helpers;
using BrightGit.Extensibility.Models;
using BrightGit.Extensibility.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.Documents;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class TabsSaveCommand : Command
{
    private readonly TraceSource logger;
    private readonly SettingsService settingsService;

    public TabsSaveCommand(TraceSource traceSource, SettingsService settingsService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.settingsService = settingsService;
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
                await shell.ShowPromptAsync("Please open a solution before saving tabs", PromptOptions.OK, cancellationToken);
                return;
            }

            // Get all opened documents.
            var openedDocuments = await documents.GetOpenDocumentsAsync(cancellationToken);

            // Check if there are any documents opened.
            if (openedDocuments.Count == 0)
            {
                await shell.ShowPromptAsync("No documents opened to save tabs", PromptOptions.OK, cancellationToken);
                return;
            }

            // Check if any document is dirty.
            foreach (var document in openedDocuments)
            {
                if (document.IsDirty)
                {
                    await shell.ShowPromptAsync("Please save all documents before saving tabs", PromptOptions.OK, cancellationToken);
                    return;
                }
            }

            // TODO: In the future find a way to figure out/save the pinned state.

            // Format the branch name.
            string gitHeadPath = Path.Combine(await VSHelper.GetSolutionDirectoryAsync(workspaces, cancellationToken), ".git", "HEAD");
            string gitBranchName = string.Empty;
            if (File.Exists(gitHeadPath))
            {
                gitBranchName = File.ReadAllText(gitHeadPath).Split('/').LastOrDefault();
                gitBranchName = $"_{gitBranchName}";
            }

            // Serialize to json.
            TabsInfo tabsInfo = new()
            {
                Id = $"TabsCustom_{solutionName}{gitBranchName}",
                Name = $"TabsCustom_{solutionName}{gitBranchName}",
                SolutionName = solutionName,
                BranchName = gitBranchName,
                DateSaved = DateTime.Now,
                Tabs = openedDocuments.Where(d => !d.IsReadOnly)
                                      .Select((d, i) => new TabDocumentInfo() { FilePath = d.Moniker.LocalPath, Index = i, IsPinned = false })
                                      .ToList()
            };
            var json = JsonSerializer.Serialize(tabsInfo);

            // Save to disk.
            await configuration.WritePersistedStateAsync(tabsInfo.Id, json, cancellationToken);
            Debug.WriteLine(json);

            // Close all documents.
            if (settingsService.Data.Tabs.CloseTabsOnSave)
            {
                // TODO: In the future use some faster way to close all documents (similar to VS -> "Close All Tabs").

                // Close all documents in parallel.
                await Task.WhenAll(openedDocuments.Select(document => document.CloseAsync(SaveDocumentOption.PromptSave, Extensibility, cancellationToken)));
            }

            sw.Stop();
            Debug.WriteLine($"Saved tabs {openedDocuments.Count} for {solutionName} ({sw.ElapsedMilliseconds}ms)");
            await shell.ShowPromptAsync($"Saved {openedDocuments.Count} tabs for {solutionName} ({sw.ElapsedMilliseconds}ms)", PromptOptions.OK, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await shell.ShowPromptAsync($"Error saving tabs: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }
}
