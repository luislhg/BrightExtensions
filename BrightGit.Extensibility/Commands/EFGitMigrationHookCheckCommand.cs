namespace BrightGit.Extensibility.Commands;

using BrightGit.Extensibility.Helpers;
using BrightGit.SharpCommon;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class EFGitMigrationHookCheckCommand : Command
{
    private readonly TraceSource logger;

    public EFGitMigrationHookCheckCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "EF Auto Migrator - Check Hook State")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.LinkAlert, IconSettings.IconAndText),
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
            var solutionDirectoryPath = await VSHelper.GetSolutionDirectoryAsync(Extensibility.Workspaces(), cancellationToken);
            if (string.IsNullOrWhiteSpace(solutionDirectoryPath))
            {
                await Extensibility.Shell().ShowPromptAsync("Please open a solution before checking the hook state.", PromptOptions.OK, cancellationToken);
                return;
            }

            var isHookActive = SharpHookHelper.CheckAutoMigratorHook(solutionDirectoryPath);
            var msg = isHookActive ? "Auto Migration hook is active for this repo." : "Auto Migration hook is NOT active for this repo.";
            await Extensibility.Shell().ShowPromptAsync(msg, PromptOptions.OK, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0, ex.Message);
            await Extensibility.Shell().ShowPromptAsync(ex.Message, PromptOptions.OK, cancellationToken);
        }
    }
}
