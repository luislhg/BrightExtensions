﻿namespace BrightGit.Extensibility;

using BrightGit.Extensibility.Commands;
using BrightGit.Extensibility.Services;
using BrightGit.Extensibility.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

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
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        // You can configure dependency injection here by adding services to the serviceCollection.
        serviceCollection.AddTransient<IDialogService>(provider => new DialogService());
        serviceCollection.AddSingleton<SettingsService>();
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
            MenuChild.Command<EFGitTest>(),
#endif
            MenuChild.Separator,
            MenuChild.Command<SettingsWindowCommand>(),
            //MenuChild.Command<HelpWindowCommand>(),
        },
    };
}