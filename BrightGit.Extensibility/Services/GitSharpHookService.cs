using BrightGit.Extensibility.Services;
using System.Diagnostics;
using System.IO.Pipes;

public class GitSharpHookService
{
    public bool IsMonitoring { get; private set; }

    private Task listeningTask;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly SettingsService settingsService;
    private readonly TabManagerService tabManagerService;
    private readonly EFCoreManagerService eFCoreManagerService;

    public GitSharpHookService(SettingsService settingsService,
                               TabManagerService tabManagerService,
                               EFCoreManagerService eFCoreManagerService)
    {
        this.settingsService = settingsService;
        this.tabManagerService = tabManagerService;
        this.eFCoreManagerService = eFCoreManagerService;
    }

    public void StartMonitoring()
    {
        if (!IsMonitoring)
        {
            listeningTask = Task.Run(() => ListenForGitHooks(cancellationTokenSource.Token));
            IsMonitoring = true;
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

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
    }
}