
using BrightXaml.Extensibility.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.ProgressReporting;
using System.Diagnostics;

namespace BrightXaml.Extensibility.Commands;

[VisualStudioContribution]
internal class TestProgressReportCommand : Command
{
    private readonly TraceSource logger;
    private readonly IDialogService dialogService;

    public TestProgressReportCommand(TraceSource traceSource, IDialogService dialogService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.dialogService = dialogService;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Test - ProgressReport")
    {
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
    };

    /// <inheritdoc />
    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        return base.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var shell = context.Extensibility.Shell();

        using (var progress = await shell.StartProgressReportingAsync("Testing Progress Report", new(true), cancellationToken))
        {
            try
            {
                const int max = 50;
                for (var i = 0; i <= max; i++)
                {
                    progress.Report(CreateProgressStatus(i, max));
                    progress.CancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(100, cancellationToken);
                }

                await shell.ShowPromptAsync("Operation completed", PromptOptions.OK, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await shell.ShowPromptAsync("Operation canceled by user.", PromptOptions.OK, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.TraceEvent(TraceEventType.Error, 0, $"Error in TestProgressDialogCommand: {ex}");
                await shell.ShowPromptAsync($"An error occurred: {ex.Message}", PromptOptions.OK, cancellationToken);
            }
        }
    }

    public static ProgressStatus CreateProgressStatus(int current, int max, string msg = "Please wait...") => new((int)(current / (double)max * 100), msg);
}
