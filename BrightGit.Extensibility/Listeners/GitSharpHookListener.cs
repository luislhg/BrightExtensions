using BrightGit.Extensibility.Services;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using System.Diagnostics;

namespace BrightGit.Extensibility.Listeners;
[VisualStudioContribution]
public class GitSharpHookListener : ExtensionPart, ITextViewOpenClosedListener, ITextViewChangedListener
{
    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromGlobPattern("**/*", true),
        ],
    };

    public GitSharpHookListener(TraceSource traceSource, SettingsService settingsService, GitSharpHookService gitSharpHookService)
    {
        // If any of the features are enabled, we need to listen for Git hooks.
        if (settingsService.Data.Tabs.IsEnabled || settingsService.Data.EFCore.IsEnabled)
        {
            // TODO:
            // Disabled at the moment as I'm experimenting with FileWatcherService.
            //_ = gitSharpHookService.StartMonitoringAsync();
        }
    }

    // We inherit from Listeners just to trigger our constructor and start monitoring git events.
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