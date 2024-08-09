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
internal class FormatXamlCommand : Command
{
    private readonly TraceSource logger;
    private readonly SettingsService settingsService;

    public FormatXamlCommand(TraceSource traceSource, SettingsService settingsService)
    {
        logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.settingsService = settingsService;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Format Xaml (Ctrl+E+F)")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.FormatDocument, IconSettings.IconAndText),
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.F)],
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorFileName, @"\.(xaml)$"),
        TooltipText = "Formats a XAML file",
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
        if (textView != null)
        {
            var textViewFilePath = textView?.FilePath;
            var textContent = new string(textView.Document.Text.ToArray());
            var originalCaret = textView.Selection.Extent.Start;

            try
            {
                // Format XAML.
                var formattedXaml = XamlFormatter.FormatXaml(textContent,
                                                             settingsService.Data.FormatXaml.EndingTagSpaces,
                                                             settingsService.Data.FormatXaml.ClosingTagSpaces);

                // Apply formatted XAML to the active document.
                await editor.EditAsync(
                batch =>
                {
                    textView.Document.AsEditable(batch).Replace(textView.Document.Text, formattedXaml);

                    // TODO: Tries to restore the caret position after formatting.
                    //var caret = new TextPosition(textView.Document, 5);
                    //textView.AsEditable(batch).SetSelections([new Selection(activePosition: caret, anchorPosition: caret, insertionPosition: caret)]);
                },
                cancellationToken);

                sw.Stop();
                Debug.WriteLine($"FormatXamlCommand.ExecuteCommandAsync: {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
                await shell.ShowPromptAsync($"Error formatting XAML.\n{ex.Message}", PromptOptions.OK, cancellationToken);
            }
        }
        else
        {
            logger.TraceEvent(TraceEventType.Warning, 0, "No active text view found.");
        }
    }
}
