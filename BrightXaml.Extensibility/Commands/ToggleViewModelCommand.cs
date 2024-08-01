namespace BrightXaml.Extensibility.Commands;

using BrightXaml.Extensibility.Services;
using BrightXaml.Extensibility.Utilities;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.ProjectSystem.Query;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class ToggleViewModelCommand : Command
{
    private readonly TraceSource logger;
    private readonly IDialogService dialogService;

    public ToggleViewModelCommand(TraceSource traceSource, IDialogService dialogService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.dialogService = Requires.NotNull(dialogService, nameof(dialogService));
        dialogService.Shell = Extensibility.Shell();
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Show View/ViewModel (Ctrl+E+Q)")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.Binding, IconSettings.IconAndText),
        //Icon = new(ImageMoniker.KnownValues.Code, IconSettings.IconAndText),
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.Q)],
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveSelectionPath, @"\.(cs|xaml)$"),

        // TODO: These don't work specifically for this command!
        //EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, @"\.(cs)$"),
        //EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, @"\.(xaml)$"),
        //EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, @"\.(cs|xaml)$"),

        TooltipText = "Show View or ViewModel",
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

        // Get active file.
        var filePath = (await context.GetSelectedPathAsync(cancellationToken)).LocalPath;
        if (filePath != null)
        {
            // Fix capitalization of file path.
            filePath = ViewModelHelper.GetProperFilePathCapitalization(filePath);

            // Get file name.
            var fileName = Path.GetFileName(filePath) ?? string.Empty;

            // Get the path of the active project.
            var activeProject = await context.GetActiveProjectAsync(cancellationToken);
            var activeProjectPath = Path.GetDirectoryName(activeProject.Path);
            if (string.IsNullOrWhiteSpace(activeProjectPath))
            {
                await dialogService.ShowPromptOKAsync("Active project path not found.", cancellationToken);
                return;
            }

            // Add \ to project path if it doesn't end with it.
            // This is needed to compare paths (check if are within same project folder).
            if (!activeProjectPath.EndsWith(Path.DirectorySeparatorChar))
                activeProjectPath += Path.DirectorySeparatorChar;

            if (fileName.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase) ||
                fileName.EndsWith(".xaml.cs", StringComparison.InvariantCultureIgnoreCase))
            {
                // Existing logic to find and open ViewModel.
                string fileNameWithoutExtension = fileName.Replace(".xaml.cs", string.Empty).Replace(".xaml", string.Empty);
                var possibleViewModels = ViewModelHelper.GetViewModelNamePossibilities(fileNameWithoutExtension);

                var files = await workspace
                    .QueryProjectsAsync(project => project
                        .Get(p => p.Files)
                        .Where(f => f.Path.StartsWith(activeProjectPath, StringComparison.InvariantCultureIgnoreCase) &&
                                    f.FileName.EndsWith("ViewModel.cs", StringComparison.InvariantCultureIgnoreCase))
                        .With(f => new { f.FileName, f.Path }), cancellationToken);

                var possibleFiles = files.Where(f => possibleViewModels.Contains(f.FileName)).Select(f => f.Path).ToList();
                if (possibleFiles.Count >= 1)
                {
                    var result = possibleFiles.FirstOrDefault();
                    sw.Stop();
                    Debug.WriteLine($"ViewModel found for '{fileName}' in {sw.ElapsedMilliseconds} ms.");

                    // If multiple files are found, prompt the user.
                    if (possibleFiles.Count >= 2)
                    {
                        result = await dialogService.ShowDialogOptionsAsync($"Found {possibleFiles.Count} matching ViewModels",
                                                                            "Choose:",
                                                                            possibleFiles,
                                                                            cancellationToken);
                    }

                    // Open the file.
                    if (result != null)
                    {
                        sw.Restart();
                        await Extensibility.Documents().OpenDocumentAsync(new Uri(result), cancellationToken);
                        sw.Stop();
                        Debug.WriteLine($"ViewModel '{Path.GetFileName(result)}' opened in {sw.ElapsedMilliseconds} ms.");
                    }
                    else
                    {
                        Debug.WriteLine($"Operation cancelled by the user.");
                    }
                }
                else
                {
                    await dialogService.ShowPromptOKAsync($"ViewModel not found for '{fileName}'.", cancellationToken);
                }
            }
            else if (fileName.EndsWith("ViewModel.cs", StringComparison.InvariantCultureIgnoreCase))
            {
                // Logic to find and open the corresponding View.
                string viewModelNameWithoutExtension = fileName.Replace(".cs", string.Empty);
                var possibleViews = ViewModelHelper.GetViewNamePossibilities(viewModelNameWithoutExtension);

                var files = await workspace
                    .QueryProjectsAsync(project => project
                        .Get(p => p.Files)
                        .Where(f => f.Path.StartsWith(activeProjectPath, StringComparison.InvariantCultureIgnoreCase) &&
                                    f.FileName.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase))
                        .With(f => new { f.FileName, f.Path }), cancellationToken);

                var possibleFiles = files.Where(f => possibleViews.Contains(f.FileName)).Select(f => f.Path).ToList();
                if (possibleFiles.Count >= 1)
                {
                    var result = possibleFiles.FirstOrDefault();
                    sw.Stop();
                    Debug.WriteLine($"View found for '{fileName}' in {sw.ElapsedMilliseconds} ms.");

                    // If multiple files are found, prompt the user.
                    if (possibleFiles.Count >= 2)
                    {
                        result = await dialogService.ShowDialogOptionsAsync($"Found {possibleFiles.Count} matching Views",
                                                                            "Choose:",
                                                                            possibleFiles,
                                                                            cancellationToken);
                    }

                    // Open the file.
                    if (result != null)
                    {
                        sw.Restart();
                        await Extensibility.Documents().OpenDocumentAsync(new Uri(result), cancellationToken);
                        sw.Stop();
                        Debug.WriteLine($"View '{Path.GetFileName(result)}' opened in {sw.ElapsedMilliseconds} ms.");
                    }
                    else
                    {
                        Debug.WriteLine($"Operation cancelled by the user.");
                    }
                }
                else
                {
                    await dialogService.ShowPromptOKAsync($"View not found for '{fileName}'.", cancellationToken);
                }
            }
            else
            {
                await dialogService.ShowPromptOKAsync("Not a XAML or ViewModel file.", cancellationToken);
            }
        }
        else
        {
            await dialogService.ShowPromptOKAsync("No active text view found.", cancellationToken);
        }
    }
}
