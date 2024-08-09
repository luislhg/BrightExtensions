namespace BrightXaml.Extensibility;

using BrightXaml.Extensibility.Commands;
using BrightXaml.Extensibility.Services;
using BrightXaml.Extensibility.Windows;
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
                id: "BrightXaml.c6b5c335-85e8-4243-bdf1-2b6468178ad1",
                version: this.ExtensionAssemblyVersion,
                publisherName: "Luis Henrique Goll",
                displayName: "Bright Xaml Extension",
                description: "Bright Commands and Automations made with Xaml Developers in mind! (WPF, MAUI, WinUI)"),
    };

    [VisualStudioContribution]
    //public static MenuConfiguration MyMenu => new("%MyMenu.DisplayName%")
    public static MenuConfiguration MyMenu => new("Bright Xaml")
    {
        Placements = new CommandPlacement[]
        {
            CommandPlacement.KnownPlacements.ExtensionsMenu
        },
        Children = new[]
        {
            MenuChild.Command<ToggleViewModelCommand>(),
            //MenuChild.Command<ShowViewCommand>(),
            //MenuChild.Command<ShowViewModelCommand>(),
            //MenuChild.Command<ShowCodeBehindCommand>(),
            MenuChild.Separator,
            MenuChild.Command<CleanBinAndObjCommand>(),
            MenuChild.Command<KillXamlDesignerCommand>(),
            MenuChild.Separator,
            MenuChild.Command<FormatXamlCommand>(),
            MenuChild.Command<FormatXamlAllCommand>(),
            //MenuChild.Separator,
            MenuChild.Command<PropertyToINPCCommand>(),
#if DEBUG
            MenuChild.Command<PropertyToINPCAllCommand>(),
            MenuChild.Separator,
            MenuChild.Command<ShowDefinitionCommand>(),
            MenuChild.Separator,
            MenuChild.Command<ExtractClassesInUseCommand>(),
            MenuChild.Command<ExtractFolderCommand>(),
#endif
            MenuChild.Separator,
            MenuChild.Command<SettingsWindowCommand>(),
            MenuChild.Command<HelpWindowCommand>(),
        },
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        // You can configure dependency injection here by adding services to the serviceCollection.
        serviceCollection.AddTransient<IDialogService>(provider => new DialogService());
        serviceCollection.AddSingleton<SettingsService>();
    }
}
