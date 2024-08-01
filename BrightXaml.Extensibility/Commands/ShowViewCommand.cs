namespace BrightXaml.Extensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class ShowViewCommand : Command
{
    private readonly TraceSource logger;

    public ShowViewCommand(TraceSource traceSource)
    {
        logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Show View (Ctrl+E+1)")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.VK_NUMPAD1)],
        Icon = new(ImageMoniker.KnownValues.View, IconSettings.IconAndText),
        TooltipText = "Shows the View of the current ViewModel",
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
        await Extensibility.Shell().ShowPromptAsync("Hello from an extension!", PromptOptions.OK, cancellationToken);
    }
}
