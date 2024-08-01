namespace BrightXaml.Extensibility.Commands;

using BrightXaml.Extensibility.Utilities;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.ProjectSystem.Query;
using System.Diagnostics;
using System.Text;
using System.Threading;

[VisualStudioContribution]
internal class ShowViewModelCommand : Command
{
    private readonly TraceSource logger;

    public ShowViewModelCommand(TraceSource traceSource)
    {
        logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Show ViewModel (Ctrl+E+2)")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.Code, IconSettings.IconAndText),
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.VK_NUMPAD2)],
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, ".+"),
        TooltipText = "Shows the ViewModel of the current View or CodeBehind",
    };

    /// <inheritdoc />
    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Use InitializeAsync for any one-time setup or initialization.
        return base.InitializeAsync(cancellationToken);
    }

    private List<string> allViewModels = [];

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var shell = Extensibility.Shell();
        var workspace = Extensibility.Workspaces();
        var documents = Extensibility.Documents();

        // TODO: Debug / Cache later.
        allViewModels = (await workspace
                    .QueryProjectsAsync(project => project
                        //.Where(p => p.Guid == knownGuid)
                        .Get(p => p.Files)
                        .Where(f => f.FileName.EndsWith("ViewModel.cs", StringComparison.InvariantCultureIgnoreCase))
                        .With(f => new { f.FileName, f.Path }),
                    cancellationToken)).Select(p => p.FileName).ToList();
        // TODO: Debug / Cache later.

        // Get current document.
        var textView = await Extensibility.Editor().GetActiveTextViewAsync(context, cancellationToken);
        if (textView != null)
        {
            var textContent = textView.Document.Text;
            var filePath = textView.FilePath;
            var fileName = Path.GetFileName(filePath) ?? string.Empty;
            if (fileName.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase) ||
                fileName.EndsWith(".xaml.cs", StringComparison.InvariantCultureIgnoreCase))
            {
                string fileNameWithoutExtension = fileName.Replace(".xaml.cs", string.Empty).Replace(".xaml", string.Empty);
                var possibleViewModels = ViewModelHelper.GetViewModelNamePossibilities(fileNameWithoutExtension);

                var files = await workspace
                    .QueryProjectsAsync(project => project
                    //.WithRequired(p => p.FilesByPath(filePath))
                    //.Where(p => p.Guid == knownGuid)
                    .Get(p => p.Files)
                    //.Get(p => p.FilesEndingWith("ViewModel.cs")
                    .Where(f => f.FileName.EndsWith("ViewModel.cs", StringComparison.InvariantCultureIgnoreCase))
                    .With(f => new { f.FileName, f.Path }),// &&
                                                           //.Where(f => possibleViewModels.Any(f.FileName, StringComparison.InvariantCultureIgnoreCase)),// &&
                                                           //f.ItemName.EndsWith("ViewModel", StringComparison.InvariantCultureIgnoreCase)), 
                    cancellationToken);

#if DEBUG
                // Debug.
                var sb = new StringBuilder();
                foreach (var file in files)
                {
                    sb.Append(file.Path + "\n");
                }
                sb.Remove(sb.Length - 2, 2);
                logger.TraceInformation(sb.ToString());
                await Extensibility.Shell().ShowPromptAsync(sb.ToString(), PromptOptions.OK, cancellationToken);
#endif

                var possibleViewModelFiles = files.Where(f => possibleViewModels.Contains(f.FileName)).Select(f => f.Path).ToList();
                if (possibleViewModelFiles.Count >= 1)
                {
                    if (possibleViewModelFiles.Count >= 2)
                    {
                        await Extensibility.Shell().ShowPromptAsync($"Multiple ViewModel files found for '{fileName}', opening the first match.", PromptOptions.OK, cancellationToken);
                    }

                    await Extensibility.Documents().OpenDocumentAsync(new Uri(possibleViewModelFiles.FirstOrDefault()), cancellationToken);
                    sw.Stop();

#if DEBUG
                    await Extensibility.Shell().ShowPromptAsync($"ViewModel found for '{fileName}' in {sw.ElapsedMilliseconds} ms.", PromptOptions.OK, cancellationToken);
#endif
                }
                else
                {
                    await Extensibility.Shell().ShowPromptAsync($"ViewModel not found for '{fileName}'.", PromptOptions.OK, cancellationToken);
                }
            }
            else
            {
                await Extensibility.Shell().ShowPromptAsync("Not a XAML file.", PromptOptions.OK, cancellationToken);
            }
        }
        else
        {
            await Extensibility.Shell().ShowPromptAsync("No active text view found.", PromptOptions.OK, cancellationToken);
        }

        //Extensibility.Workspaces().QueryProjectsAsync

        // Is not working (VS 2022 regular... try later in Preview)
        //var activeView = await context.GetActiveTextViewAsync(cancellationToken);
        //var activePath = await context.GetSelectedPathAsync(cancellationToken);

        // In-proc only (DTE).
        //var currentView = await this.GetCurrentTextViewAsync();
    }
}
