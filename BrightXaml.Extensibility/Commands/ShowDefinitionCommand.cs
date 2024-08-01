namespace BrightXaml.Extensibility.Commands;

using BrightXaml.Extensibility.Services;
using BrightXaml.Extensibility.Utilities;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.ProjectSystem.Query;
using Microsoft.VisualStudio.RpcContracts.OpenDocument;
using Microsoft.VisualStudio.RpcContracts.Utilities;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class ShowDefinitionCommand : Command
{
    private readonly TraceSource logger;
    private readonly IDialogService dialogService;

    public ShowDefinitionCommand(TraceSource traceSource, IDialogService dialogService)
    {
        logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.dialogService = Requires.NotNull(dialogService, nameof(dialogService));
        dialogService.Shell = Extensibility.Shell();
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "[WIP] Go To Binding Definition (Ctrl+E+D)")
    {
        // TODO: This maybe could have the same F12 shortcut than VS and only take priority if it's inside a XAML file and even perhaps inside a binding expression?

        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.Binding, IconSettings.IconAndText),
        //Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.F12)],   // Doesn't work?
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.D)],
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorFileName, @"\.(xaml)$"),
        TooltipText = "Navigates to the real definition of the binding",
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

        // Get active document editor.
        var textView = await editor.GetActiveTextViewAsync(context, cancellationToken);
        var textViewFilePath = textView?.FilePath;
        var chars = textView.Document.Text.ToList();
        var textContent = new string(textView.Document.Text.ToArray());
        var textStart = textView.Selection.Start;
        var textEnd = textView.Selection.End;
        var textAnchor = textView.Selection.AnchorPosition;
        var textActive = textView.Selection.ActivePosition;
        var insertionPosition = textView.Selection.InsertionPosition;
        var insertionPositionOffset = textView.Selection.InsertionPosition.Offset;
        var line = textView.Document.GetLineFromPosition(insertionPositionOffset);
        var lineNumber = line.LineNumber;
        var lineContent = new string(line.Text.ToArray());

        Debug.WriteLine("insertionPosition: " + insertionPosition.ToString());
        Debug.WriteLine("insertionPositionOffset: " + insertionPositionOffset.ToString());
        Debug.WriteLine("line: " + line.LineNumber);
        Debug.WriteLine("lineContent: " + lineContent);

        // Calculate the start offset of the line.
        var lineStartOffset = textContent.LastIndexOf('\n', insertionPositionOffset - 1) + 1;
        if (lineStartOffset == 0 && insertionPositionOffset == 0)
        {
            lineStartOffset = 0;
        }
        Debug.WriteLine("lineStartOffset: " + lineStartOffset);

        // Calculate the offset of the caret in the line.
        var lineCaretOffset = insertionPositionOffset - lineStartOffset;
        Debug.WriteLine("lineCaretOffset: " + lineCaretOffset);

        try
        {
            // Extract binding path from line.
            var bindingWord = ShowDefinitionHelper.GetWordAtCaret(lineContent, lineCaretOffset);
            Debug.WriteLine("bindingWord: " + bindingWord);
            if (bindingWord != null)
            {
                if (ShowDefinitionHelper.IsWordBindingCommand(bindingWord))
                {
                    // Get the view model for the active document.
                    // Get the path of the active project.
                    var activeProject = await context.GetActiveProjectAsync(cancellationToken);
                    var activeProjectPath = Path.GetDirectoryName(activeProject.Path);
                    if (string.IsNullOrWhiteSpace(activeProjectPath))
                    {
                        await shell.ShowPromptAsync("Active project path not found.", PromptOptions.OK, cancellationToken);
                        return;
                    }

                    // Add \ to project path if it doesn't end with it.
                    // This is needed to compare paths (check if are within same project folder).
                    if (!activeProjectPath.EndsWith(Path.DirectorySeparatorChar))
                        activeProjectPath += Path.DirectorySeparatorChar;

                    // Existing logic to find and open ViewModel.
                    var fileName = Path.GetFileName(textViewFilePath);
                    string fileNameWithoutExtension = fileName.Replace(".xaml", string.Empty);
                    var possibleViewModels = ViewModelHelper.GetViewModelNamePossibilities(fileNameWithoutExtension);

                    var files = await workspace
                        .QueryProjectsAsync(project => project
                            .Get(p => p.Files)
                            .Where(f => f.Path.StartsWith(activeProjectPath, StringComparison.InvariantCultureIgnoreCase) &&
                                        f.FileName.EndsWith("ViewModel.cs", StringComparison.InvariantCultureIgnoreCase))
                            .With(f => new { f.FileName, f.Path }), cancellationToken);

                    var possibleFiles = files.Where(f => possibleViewModels.Contains(f.FileName)).Select(f => f.Path).ToList();
                    var result = possibleFiles.FirstOrDefault();
                    if (result != null)
                    {
                        // NOTE: We might have several kinds of command declaration, we'll focus on the MVVM Community Toolkit pattern.
                        // Find this binding path in the view model.
                        var bindingWordOffset = ShowDefinitionHelper.GetRelayCommandOffset_MVVMToolkit(bindingWord, result);
                        var bindingWordLine = ShowDefinitionHelper.GetRelayCommandLine_MVVMToolkit(bindingWord, result);
                        Debug.WriteLine("bindingWordOffset: " + bindingWordOffset);
                        Debug.WriteLine("bindingWordLine: " + bindingWordLine);
                        if (bindingWordOffset >= 0)
                        {
                            // Open the file where the binding is defined.
                            var options = new OpenDocumentOptions(activate: true, ensureVisible: new Range(bindingWordLine, 0, 0, 0), ensureVisibleOptions: EnsureRangeVisibleOptions.MinimumScroll);
                            var openedDocument = await Extensibility.Documents().OpenTextDocumentAsync(new Uri(result), options, cancellationToken);
                            var openedTextView = await Extensibility.Editor().GetActiveTextViewAsync(context, cancellationToken);

                            // TODO: Navigate (caret/scroll) to the binding definition, isn't working yet.
                            await Extensibility.Editor().EditAsync(
                            batch =>
                            {
                                var caret = new TextPosition(openedTextView.Document, bindingWordOffset);
                                openedTextView.AsEditable(batch).SetSelections([new Selection(activePosition: caret, anchorPosition: caret, insertionPosition: caret)]);
                            },
                            cancellationToken);

                            sw.Stop();
                            Debug.WriteLine($"Opened binding definition for {bindingWord} ({sw.ElapsedMilliseconds}ms)");
                        }
                        else
                        {
                            logger.TraceEvent(TraceEventType.Warning, 0, $"Binding for {bindingWord} not found in '{result}'.");
                            await dialogService.ShowPromptOKAsync($"Binding for {bindingWord} not found in '{result}'.", cancellationToken);
                        }
                    }
                    else
                    {
                        logger.TraceEvent(TraceEventType.Warning, 0, $"ViewModel not found for '{fileName}'.");
                        await dialogService.ShowPromptOKAsync($"ViewModel not found for '{fileName}'.", cancellationToken);
                    }
                }
                else
                {
                    logger.TraceEvent(TraceEventType.Warning, 0, "Only a Command binding is supported at the moment.");
                    await shell.ShowPromptAsync("Only a Command binding is supported at the moment.", PromptOptions.OK, cancellationToken);
                }
            }
            else
            {
                logger.TraceEvent(TraceEventType.Warning, 0, "No binding found at the specified position.");
                await shell.ShowPromptAsync("No binding found at the specified position.", PromptOptions.OK, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0, ex.Message);
            await shell.ShowPromptAsync($"Error showing binding definition: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }
}
