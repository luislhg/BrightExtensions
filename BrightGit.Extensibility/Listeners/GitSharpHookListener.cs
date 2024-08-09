using BrightGit.Extensibility.Services;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using System.Diagnostics;
using System.IO.Pipes;

namespace BrightGit.Extensibility.Listeners;
[VisualStudioContribution]
public class GitSharpHookListener : ExtensionPart, ITextViewOpenClosedListener, ITextViewChangedListener
{
    private readonly Task listeningTask;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly SettingsService settingsService;

    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromGlobPattern("**/*", true),
        ],
    };

    // TODO: Still need to figure out how to make this listener work in the background for this API.
    public GitSharpHookListener(TraceSource traceSource, SettingsService settingsService)
    {
        listeningTask = Task.Run(() => ListenForGitHooks(cancellationTokenSource.Token));
        this.settingsService = settingsService;
    }

    private void ListenForGitHooks(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using (var pipeServer = new NamedPipeServerStream("BrightSharpHook", PipeDirection.In))
            {
                try
                {
                    pipeServer.WaitForConnection();
                    using (var reader = new StreamReader(pipeServer))
                    {
                        string message = reader.ReadLine();
                        if (message != null)
                        {
                            HandleGitHookMessage(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    // Handle exceptions (e.g., log the error).
                }
            }
        }
    }

    private void HandleGitHookMessage(string message)
    {
        string[] parts = message.Split(':');
        if (parts.Length == 2)
        {
            string eventName = parts[0];
            string branchName = parts[1];

            SaveAndRestoreTabs(branchName);
            EFDatabaseMigration(branchName);
        }
    }

    private void SaveAndRestoreTabs(string branchName)
    {
        // Ignore if the feature is disabled.
        if (!settingsService.Data.Tabs.IsEnabled)
            return;

        // Implement logic to save and restore tabs based on the branchName.
    }

    private void EFDatabaseMigration(string branchName)
    {
        // Ignore if the feature is disabled.
        if (!settingsService.Data.EFCore.IsEnabled)
            return;

        // Implement logic to run EF migrations based on the branchName.
    }

    public new void Dispose()
    {
        cancellationTokenSource.Cancel();
        base.Dispose();
    }

    // We inherit from listener just to trigger our constructor.
    public Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task TextViewClosedAsync(ITextViewSnapshot textView, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task TextViewOpenedAsync(ITextViewSnapshot textView, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}