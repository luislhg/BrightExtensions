namespace BrightXaml.Extensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class ShowCodeBehindCommand : Command
{
    private readonly TraceSource logger;

    public ShowCodeBehindCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Show CodeBehind (Ctrl+E+3)")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.VK_NUMPAD3)],
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, ".+"),
        TooltipText = "Shows the CodeBehind of the current View or ViewModel",
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
        await this.Extensibility.Shell().ShowPromptAsync("Hello from Luis :)", PromptOptions.OK, cancellationToken);
    }
}
