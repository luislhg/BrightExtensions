using BrightGit.Extensibility.Helpers;
using BrightGit.SharpCommon.Helpers;
using LibGit2Sharp;
using Microsoft.VisualStudio.Extensibility;
using System.Diagnostics;

namespace BrightGit.Extensibility.Services;
public class GitFileWatcherService
{
    // Provided at startup by the Extension.
    public VisualStudioExtensibility Extensibility { get; set; }

    public string SolutionDir { get; private set; }
    public string CurrentBranchName { get; private set; }

    private FileSystemWatcher watcher;

    public bool IsMonitoring { get; private set; }

    private readonly TraceSource logger;
    private readonly SettingsService settingsService;
    private readonly TabManagerService tabManagerService;
    private readonly EFCoreManagerService efCoreManagerService;

    public GitFileWatcherService(TraceSource traceSource,
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

                // Setup file system watcher.
                watcher = new FileSystemWatcher
                {
                    Path = Path.Combine(SolutionDir, ".git"),
                    Filter = "*",
                    //Filter = "HEAD",
                };
                //watcher.Changed += OnFileChanged;
                //watcher.Created += OnFileCreatedOrDeleted;
                //watcher.Deleted += OnFileCreatedOrDeleted;
                watcher.Renamed += OnFileRenamed;
                watcher.EnableRaisingEvents = true;

                IsMonitoring = true;
                logger.TraceInformation("Started monitoring Git HEAD.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
            }
        }
        else
        {
            Debug.WriteLine("Already monitoring Git HEAD.");
            logger.TraceInformation("Already monitoring Git HEAD.");
        }
    }

    public void StopMonitoring()
    {
        if (IsMonitoring)
        {
            watcher.Dispose();
            IsMonitoring = false;
            logger.TraceInformation("Stopped monitoring Git HEAD.");
        }
    }

    private void OnFileCreatedOrDeleted(object sender, FileSystemEventArgs e)
    {
        Debug.WriteLine($"File {e.ChangeType}: {e.FullPath}");
        logger.TraceInformation($"File {e.ChangeType}: {e.FullPath}");
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        Debug.WriteLine($"File renamed from {e.OldFullPath} to {e.FullPath}");
        logger.TraceInformation($"File renamed from {e.OldFullPath} to {e.FullPath}");

        if (e.OldName == "HEAD.lock" && e.Name == "HEAD")
        {
            ReadCurrentBranchName();
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Debug.WriteLine($"File {e.ChangeType}: {e.FullPath}");
        logger.TraceInformation($"File {e.ChangeType}: {e.FullPath}");
    }

    private void ReadCurrentBranchName()
    {
        try
        {
            using (var repo = new Repository(SolutionDir))
            {
                var oldBranchName = CurrentBranchName;
                var currentBranch = repo.Head.FriendlyName;
                if (currentBranch != CurrentBranchName)
                {
                    CurrentBranchName = currentBranch;
                    Debug.WriteLine($"Branch changed from {oldBranchName} to {currentBranch}.");
                    logger.TraceInformation($"Branch changed from {oldBranchName} to {currentBranch}.");

                    // Save and restore tabs.
                    tabManagerService.Extensibility = Extensibility;
                    _ = tabManagerService.SaveAndRestoreTabsAsync(SolutionDir, oldBranchName, CurrentBranchName);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
        }
    }
}
