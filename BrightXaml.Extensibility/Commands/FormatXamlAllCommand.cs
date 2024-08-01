namespace BrightXaml.Extensibility.Commands;

using BrightXaml.Extensibility.Services;
using BrightXaml.Extensibility.Utilities;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class FormatXamlAllCommand : Command
{
    private readonly TraceSource logger;
    private readonly IDialogService dialogService;

    public FormatXamlAllCommand(TraceSource traceSource, IDialogService dialogService)
    {
        logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.dialogService = dialogService;
        this.dialogService.Shell = Extensibility.Shell();
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Format All Xaml (Folder/Project)")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.FormatDocument, IconSettings.IconAndText),
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.F)],
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveSelectionPath, ".+"),
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
        var sw = Stopwatch.StartNew();
        var shell = Extensibility.Shell();
        var workspace = Extensibility.Workspaces();
        var documents = Extensibility.Documents();
        var editor = Extensibility.Editor();

        // Get the path of the active project.
        var activeProject = await context.GetActiveProjectAsync(cancellationToken);
        var activeProjectPath = activeProject?.Path;
        var activeProjectFilename = Path.GetFileName(activeProjectPath);
        var activeProjectDir = Path.GetDirectoryName(activeProject?.Path);
        if (string.IsNullOrWhiteSpace(activeProjectDir))
        {
            await shell.ShowPromptAsync("Active project path not found.", PromptOptions.OK, cancellationToken);
            return;
        }
        var xamlFilesInProject = Directory.GetFiles(activeProjectDir, "*.xaml", SearchOption.AllDirectories);

        // Get active file.
        var filePath = (await context.GetSelectedPathAsync(cancellationToken)).LocalPath;
        if (filePath != null)
        {
            // Fix capitalization of file path.
            filePath = ViewModelHelper.GetProperFilePathCapitalization(filePath);

            // Get directory path.
            var directoryPath = Path.GetDirectoryName(filePath);
            var directoryName = Path.GetFileName(directoryPath);
            var xamlFilesInFolder = Directory.GetFiles(directoryPath, "*.xaml", SearchOption.TopDirectoryOnly);

            var options = new List<string>() { "Project", "Folder", "Cancel" };
            var projectMsg = $"Project: {activeProjectFilename} ({xamlFilesInProject.Length} files)";
            var folderMsg = $"Folder: {directoryName} ({xamlFilesInFolder.Length} files)";
            var result = await dialogService.ShowPromptOptionsAsync($"Format all XAML in{Environment.NewLine}{Environment.NewLine}{projectMsg}{Environment.NewLine}{Environment.NewLine}{folderMsg}",
                                                                    options,
                                                                    cancellationToken);
            if (result == "Project")
            {
                var swProject = Stopwatch.StartNew();
                await FormatAllFilesAsync(shell, xamlFilesInProject, cancellationToken);
                swProject.Stop();
                await dialogService.ShowPromptOKAsync($"Formatted {xamlFilesInProject.Length} XAML files in Project ({swProject.ElapsedMilliseconds}ms)", cancellationToken);
            }
            else if (result == "Folder")
            {
                var swFolder = Stopwatch.StartNew();
                await FormatAllFilesAsync(shell, xamlFilesInFolder, cancellationToken);
                swFolder.Stop();
                await dialogService.ShowPromptOKAsync($"Formatted {xamlFilesInFolder.Length} XAML files in Folder ({swFolder.ElapsedMilliseconds}ms)", cancellationToken);
            }
        }
    }

    private async Task FormatAllFilesAsync(ShellExtensibility shell, string[] xamlFiles, CancellationToken cancellationToken)
    {
        foreach (var file in xamlFiles)
        {
            try
            {
                await XamlFormatter.FormatFileAsync(file, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
                var fileName = Path.GetFileName(file);
                await shell.ShowPromptAsync($"Error formatting XAML {fileName} .\n{ex.Message}", PromptOptions.OK, cancellationToken);
            }
        }
    }
}
