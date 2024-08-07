namespace BrightGit.Extensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.Documents;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class TabsSortCommand : Command
{
    private readonly TraceSource logger;

    public TabsSortCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "(WIP) Sort Tabs")
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
        try
        {
            var sw = Stopwatch.StartNew();

            var openedDocuments = await Extensibility.Documents().GetOpenDocumentsAsync(cancellationToken);
            if (openedDocuments.Any())
            {
                var sortedDocuments = openedDocuments.OrderBy(d => Path.GetFileName(d.Moniker.LocalPath)).ToList();

                // TODO: This is a terrible way to do this... I couldn't find a better way to simply reorder the tabs with the API.

                // Close all documents.
                //await Task.WhenAll(openedDocuments.Select(document => document.CloseAsync(SaveDocumentOption.PromptSave, Extensibility, cancellationToken)));
                await Task.WhenAll(openedDocuments.Select(document => Extensibility.Documents().CloseDocumentAsync(document.Moniker, SaveDocumentOption.PromptSave, cancellationToken)));

                // Open documents in sorted order.
                await Task.WhenAll(sortedDocuments.Select(document => Extensibility.Documents().OpenDocumentAsync(document.Moniker, cancellationToken)));

                sw.Stop();
                Debug.WriteLine($"Sorted {openedDocuments.Count} tabs in {sw.ElapsedMilliseconds}ms");
                await Extensibility.Shell().ShowPromptAsync($"Sorted {openedDocuments.Count} tabs in {sw.ElapsedMilliseconds}ms", PromptOptions.OK, cancellationToken);
            }
            else
            {
                await Extensibility.Shell().ShowPromptAsync("No tabs to sort", PromptOptions.OK, cancellationToken);
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Extensibility.Shell().ShowPromptAsync("An error occurred while sorting tabs", PromptOptions.OK, cancellationToken);
        }
    }
}
