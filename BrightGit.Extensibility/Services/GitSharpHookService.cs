using BrightGit.Extensibility.Helpers;
using BrightGit.Extensibility.Services;
using BrightGit.SharpCommon;
using BrightGit.SharpCommon.Helpers;
using LibGit2Sharp;
using Microsoft.VisualStudio.Extensibility;
using System.Diagnostics;
using System.IO.Pipes;

public class GitSharpHookService
{
    // Provided at startup by the Extension.
    public VisualStudioExtensibility Extensibility { get; set; }

    public string SolutionDir { get; private set; }
    public string CurrentBranchName { get; private set; }

    public bool IsMonitoring { get; private set; }

    private Task listeningTask;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly TraceSource logger;
    private readonly SettingsService settingsService;
    private readonly TabManagerService tabManagerService;
    private readonly EFCoreManagerService efCoreManagerService;

    public GitSharpHookService(TraceSource traceSource,
                               SettingsService settingsService,
                               TabManagerService tabManagerService,
                               EFCoreManagerService eFCoreManagerService)
    {
        this.logger = traceSource;
        this.settingsService = settingsService;
        this.tabManagerService = tabManagerService;
        this.efCoreManagerService = eFCoreManagerService;
    }

    public async Task StartMonitoringAsync()
    {
        if (!IsMonitoring)
        {
            try
            {
                SolutionDir = await VSHelper.GetSolutionDirectoryAsync(Extensibility.Workspaces(), CancellationToken.None);
                CurrentBranchName = GitHelper.GetCurrentBranchName(SolutionDir);

                // Setup git hook.
                SharpHookHelper.AddSharpHook(SolutionDir);

                // Start listening for Git hooks.
                listeningTask = Task.Run(() => ListenForGitHooks(cancellationTokenSource.Token));
                IsMonitoring = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        else
        {
            Debug.WriteLine("Already monitoring Git hooks.");
        }
    }

    public async Task StopMonitoringAsync()
    {
        if (IsMonitoring)
        {
            await cancellationTokenSource.CancelAsync();
            IsMonitoring = false;
        }
        else
        {
            Debug.WriteLine("Not monitoring Git hooks.");
        }
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
        string[] parts = message.Split('|');
        if (parts.Length >= 1)
        {
            string eventName = parts[0];

            // Implement logic based on the eventName.
            if (eventName == "PostCheckout" && parts.Length >= 4)
            {
                string repoDir = parts[1];
                string oldRef = parts[2];
                string newRef = parts[3];

                using (var repo = new Repository(repoDir))
                {
                    var oldCommit = repo.Lookup<Commit>(oldRef);
                    var newCommit = repo.Lookup<Commit>(newRef);
                    string oldBranchName = GitHelper.GetBranchNameFromCommit(repo, oldCommit);
                    string newBranchName = GitHelper.GetBranchNameFromCommit(repo, newCommit);

                    if (!string.IsNullOrEmpty(oldBranchName) && !string.IsNullOrEmpty(newBranchName))
                    {
                        Debug.WriteLine($"Branch names found for commits {oldRef} ({oldBranchName}) and {newRef} ({newBranchName})");

                        SaveAndRestoreTabs(repoDir, oldBranchName, newBranchName);
                        EFDatabaseMigration(oldRef, newRef);
                    }
                    else
                    {
                        Debug.WriteLine($"Branch names not found for commits {oldRef} and {newRef}");
                        logger.TraceEvent(TraceEventType.Information, 0, $"Branch names not found for commits {oldRef} and {newRef}");
                    }
                }
            }
            else
            {
                Debug.WriteLine($"Unknown event name or invalid lengh: {eventName} ({parts.Length})");
            }
        }
        else
        {
            Debug.WriteLine("Invalid message format. Length is 0.");
        }
    }

    private void SaveAndRestoreTabs(string repoDir, string oldBranch, string newBranch)
    {
        // Ignore if the feature is disabled.
        //if (!settingsService.Data.Tabs.IsEnabled)
        //    return;

        // Implement logic to save and restore tabs based on the branchName.
        tabManagerService.Extensibility = Extensibility;
        _ = tabManagerService.SaveAndRestoreTabsAsync(repoDir, oldBranch, newBranch);
    }

    private void EFDatabaseMigration(string oldRef, string newRef)
    {
        // Ignore if the feature is disabled.
        if (!settingsService.Data.EFCore.IsEnabled)
            return;

        // Implement logic to run EF migrations based on the branchName.
    }

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
    }
}