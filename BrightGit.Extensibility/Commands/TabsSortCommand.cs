namespace BrightGit.Extensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class TabSortCommand : Command
{
    private readonly TraceSource logger;

    public TabSortCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Sort Tabs")
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
        var openedDocuments = await Extensibility.Documents().GetOpenDocumentsAsync(cancellationToken);
        var sortedDocuments = openedDocuments.OrderBy(d => Path.GetFileName(d.Moniker.LocalPath)).ToList();

        // TODO: This is a terrible way to do this... I couldn't find a better way to simply reorder the tabs with the API.

        // Close all documents.
        foreach (var document in openedDocuments)
        {
            await Extensibility.Documents().CloseDocumentAsync(document.Moniker, Microsoft.VisualStudio.RpcContracts.Documents.SaveDocumentOption.PromptSave, cancellationToken);
        }

        // Open documents in sorted order.
        foreach (var document in sortedDocuments)
        {
            await Extensibility.Documents().OpenDocumentAsync(document.Moniker, cancellationToken);
        }
    }
}
