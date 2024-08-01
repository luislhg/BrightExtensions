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
internal class EFGitMigrationHookAddCommand : Command
{
    private readonly TraceSource logger;

    public EFGitMigrationHookAddCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "EF Auto Migrator - Add Hook")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.AddLink, IconSettings.IconAndText),
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
                await Extensibility.Shell().ShowPromptAsync("Please open a solution before adding the hook.", PromptOptions.OK, cancellationToken);
                return;
            }

            SharpHookHelper.AddAutoMigratorHook(solutionDirectoryPath);
            await Extensibility.Shell().ShowPromptAsync($"Auto Migration hook added to repo {Path.GetDirectoryName(solutionDirectoryPath)}.", PromptOptions.OK, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0, ex.Message);
            await Extensibility.Shell().ShowPromptAsync(ex.Message, PromptOptions.OK, cancellationToken);
        }
    }
}
