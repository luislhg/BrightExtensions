using Microsoft.VisualStudio.Extensibility;
using System.Diagnostics;

namespace BrightGit.Extensibility.Services;

public class EFCoreManagerService
{
    // TODO: Maybe this should be injected and then EFCoreManagerService would need to be Scoped (accordingly to samples/doc in VS API).
    public VisualStudioExtensibility Extensibility { get; set; }

    private readonly TraceSource logger;
    private readonly SettingsService settingsService;

    public EFCoreManagerService(TraceSource logger, SettingsService settingsService)
    {
        this.logger = logger;
        this.settingsService = settingsService;
    }

    public Task<bool> CheckMigrationsAsync(string solutionDir, string oldBranchName, string currentBranchName)
    {

        return Task.FromResult(true);
    }
}
