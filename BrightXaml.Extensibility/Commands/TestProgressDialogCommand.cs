
using BrightXaml.Extensibility.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;

namespace BrightXaml.Extensibility.Commands;

[VisualStudioContribution]
internal class TestProgressDialogCommand : Command
{
    private readonly TraceSource logger;
    private readonly IDialogService dialogService;

    public TestProgressDialogCommand(TraceSource traceSource, IDialogService dialogService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.dialogService = dialogService;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Test - ProgressDialog")
    {
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
            // Start showing the progress dialog (don't await yet, we need to update progress while it's open)
            dialogService.Shell = context.Extensibility.Shell();
            var dialogTask = dialogService.ShowDialogProgressAsync("Processing items...", out var updateProgress, cancellationToken);

            // Simulate work with progress updates
            for (int i = 0; i <= 100; i += 1)
            {
                updateProgress(i, false);
                await Task.Delay(50, cancellationToken);
            }

            // Indicate that the progress is complete
            updateProgress(100, true);

            // Wait for the user to close the dialog
            await dialogTask;

            await this.Extensibility.Shell().ShowPromptAsync("Finished executing!", PromptOptions.OK, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            await this.Extensibility.Shell().ShowPromptAsync("Operation canceled by user.", PromptOptions.OK, cancellationToken);
        }
        catch (ObjectDisposedException)
        {
            // Usually this means the op was cancelled by the user (closed the window) and the sync just failed.
            await this.Extensibility.Shell().ShowPromptAsync("* Operation canceled by user.", PromptOptions.OK, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0, $"Error in TestProgressDialogCommand: {ex}");
            await this.Extensibility.Shell().ShowPromptAsync($"An error occurred: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }
}
