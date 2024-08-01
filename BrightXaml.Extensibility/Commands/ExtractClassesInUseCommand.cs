namespace BrightXaml.Extensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class ExtractClassesInUseCommand : Command
{
    private readonly TraceSource logger;

    public ExtractClassesInUseCommand(TraceSource traceSource)
    {
        logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "[WIP] Export Referenced Classes")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.ExportData, IconSettings.IconAndText),
        //Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.H)],
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorFileName, @"\.(cs)$"),
        TooltipText = "Copies to clipboard all classes being used in the current document",
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
        var workspace = Extensibility.Workspaces();
        var documents = Extensibility.Documents();

        await Extensibility.Shell().ShowPromptAsync("Work in Progress!", PromptOptions.OK, cancellationToken);
    }

    public void CopyClassesToClipboard(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {

        }
    }
}
