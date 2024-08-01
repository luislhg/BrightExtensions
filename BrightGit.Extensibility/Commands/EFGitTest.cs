namespace BrightGit.Extensibility.Commands;
using LibGit2Sharp;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class EFGitTest : Command
{
    private readonly TraceSource logger;

    public EFGitTest(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "(Dev) EF Core - Test Git Library")
    {
        // Use in debug only for testing, might be deleted later.
#if DEBUG
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
#endif
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
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
            // Set the native library path for LibGit2Sharp when inside VS extension (VSIX).
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            assemblyFolder = assemblyFolder[..^2];
            GlobalSettings.NativeLibraryPath = Path.Combine(assemblyFolder, "runtimes", "win-x64", "native");

            var repo = new Repository("");
            await Extensibility.Shell().ShowPromptAsync("Success", PromptOptions.OK, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0, ex.Message);
            await Extensibility.Shell().ShowPromptAsync(ex.Message, PromptOptions.OK, cancellationToken);
        }
    }
}
