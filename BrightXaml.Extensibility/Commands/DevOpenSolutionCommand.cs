namespace BrightXaml.Extensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class DevOpenSolutionCommand : Command
{
    private readonly TraceSource logger;

    public DevOpenSolutionCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Dev Open Test Solution")
    {
#if DEBUG
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        //Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.T)],
#endif
        Icon = new(ImageMoniker.KnownValues.DebugTemplate, IconSettings.IconAndText),
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
        // TODO: Open a solution for testing.
        // TODO: This opens the file, not the solution itself...
        await this.Extensibility.Documents().OpenDocumentAsync(new Uri(@"D:\Projects Visual Studio\Test Projects\TestWPFNETCore\TestWPFNETCore.sln"), cancellationToken);
    }
}
