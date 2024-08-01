namespace BrightXaml.Extensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class KillXamlDesignerCommand : Command
{
    private readonly TraceSource logger;

    public KillXamlDesignerCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Kill XAML Designer Process")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.TerminateProcess, IconSettings.IconAndText),
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
        KillAllDesigners();
        await Task.CompletedTask;
    }

    private static void KillAllDesigners()
    {
        // VS 2022.
        var designers = Process.GetProcessesByName("WpfSurface");
        foreach (var designer in designers)
        {
            designer.Kill();
        }

        // Usually up to VS 2019.
        //var designers = Process.GetProcessesByName("XDesProc");
        //foreach (var designer in designers)
        //{
        //    designer.Kill();
        //}
    }
}
