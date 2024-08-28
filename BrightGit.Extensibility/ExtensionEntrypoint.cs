namespace BrightGit.Extensibility;

using BrightGit.Extensibility.Commands;
using BrightGit.Extensibility.Listeners;
using BrightGit.Extensibility.Services;
using BrightGit.Extensibility.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.ProjectSystem.Query;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Extension entrypoint for the VisualStudio.Extensibility extension.
/// </summary>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
                id: "BrightGit.14722f3b-45e7-4e62-bfde-b25896550871",
                version: this.ExtensionAssemblyVersion,
                publisherName: "Luis Henrique Goll",
                displayName: "Bright Git Extension",
                description: "Bright Commands and Automations with C# developers using git source control in mind!"),
        LoadedWhen = ActivationConstraint.SolutionState(SolutionState.FullyLoaded),
        //LoadedWhen = ActivationConstraint.SolutionState(SolutionState.Exists)
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        // You can configure dependency injection here by adding services to the serviceCollection.
        serviceCollection.AddTransient<IDialogService>(provider => new DialogService());
        serviceCollection.AddSingleton<SettingsService>();
        serviceCollection.AddSingleton<TabsStorageService>();
        serviceCollection.AddSingleton<GitSharpHookService>();
        serviceCollection.AddSingleton<GitFileWatcherService>();
        serviceCollection.AddSingleton<EFCoreManagerService>();
        serviceCollection.AddSingleton<TabManagerService>();
    }

    protected override async Task OnInitializedAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        // Start monitoring Git hooks.
        //base.ServiceProvider.GetRequiredService<GitSharpHookService>().Extensibility = extensibility;
        //_ = base.ServiceProvider.GetRequiredService<GitSharpHookService>().StartMonitoringAsync();

        // Start monitoring Git HEAD.
        base.ServiceProvider.GetRequiredService<GitFileWatcherService>().Extensibility = extensibility;
        _ = base.ServiceProvider.GetRequiredService<GitFileWatcherService>().StartMonitoringAsync();

        // Subscribe to solution changes.
        var solutions = await extensibility.Workspaces().QuerySolutionAsync(solution => solution.With(p => p.Path), cancellationToken);
        var firstSolution = solutions.FirstOrDefault();
        if (firstSolution != null)
        {
            var observer = new SolutionSubscriptionObserver();
            var subscribe = firstSolution.Projects.With(p => p.Path).SubscribeAsync(observer, CancellationToken.None);
        }

        // Carry on with the initialization.
        await base.OnInitializedAsync(extensibility, cancellationToken);
    }

    [VisualStudioContribution]
    //public static MenuConfiguration MyMenu => new("%MyMenu.DisplayName%")
    public static MenuConfiguration MyMenu => new("Bright Git")
    {
        Placements = new[]
        {
            CommandPlacement.KnownPlacements.ExtensionsMenu
        },
        Children = new[]
        {
            MenuChild.Command<EFGitMigrationDownCommand>(),
            MenuChild.Separator,
            MenuChild.Command<EFGitMigrationHookAddCommand>(),
            MenuChild.Command<EFGitMigrationHookRemoveCommand>(),
            MenuChild.Command<EFGitMigrationHookCheckCommand>(),

            MenuChild.Separator,
            MenuChild.Command<TabsSaveCommand>(),
            MenuChild.Command<TabsRestoreCommand>(),
#if DEBUG
            MenuChild.Separator,
            MenuChild.Command<TabsSortCommand>(),
            MenuChild.Command<EFGitTest>(),
#endif
            MenuChild.Separator,
            MenuChild.Command<SettingsWindowCommand>(),
#if DEBUG
            MenuChild.Command<TabsWindowCommand>(),
#endif
            //MenuChild.Command<HelpWindowCommand>(),
        },
    };
}
