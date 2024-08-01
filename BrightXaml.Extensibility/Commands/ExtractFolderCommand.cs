namespace BrightXaml.Extensibility.Commands;

using BrightXaml.Extensibility.Helpers;
using BrightXaml.Extensibility.Utilities;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class ExtractFolderCommand : Command
{
    private readonly TraceSource logger;

    public ExtractFolderCommand(TraceSource traceSource)
    {
        logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Export Folder")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.ExportData, IconSettings.IconAndText),
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.J)],
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, ".+"),
        TooltipText = "Copies to clipboard all coding files in the same folder of the current document",
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
        var shell = Extensibility.Shell();
        var workspace = Extensibility.Workspaces();
        var documents = Extensibility.Documents();

        // Get current document.
        var textView = await Extensibility.Editor().GetActiveTextViewAsync(context, cancellationToken);
        var filePath = textView?.FilePath;
        if (textView != null && !string.IsNullOrWhiteSpace(filePath))
        {
            try
            {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (directoryPath != null)
                {
                    var sw = Stopwatch.StartNew();
                    var csFilenames = ExportFilesHelper.GetFilesFromDir(directoryPath, false, [".cs", ".xaml"]);
                    if (csFilenames != null && csFilenames.Count > 0)
                    {
                        var allContent = await ExportFilesHelper.ReadFilesContentAsync(csFilenames, true, true);

                        if (allContent != null)
                        {
                            // Copy the content to the clipboard.
                            ClipboardHelper.SetClipboard(allContent);
                            sw.Stop();

                            // Display in the Prompt: the folder path, each file name, and the time taken to copy the contents.
                            var message = new StringBuilder()
                                .AppendLine($"Copied files from Folder: {directoryPath}")
                                .AppendLine()
                                .AppendJoin(Environment.NewLine, csFilenames.Select(file => $"{file}"))
                                .AppendLine()
                                .AppendLine()
                                .AppendLine($"Total: {csFilenames.Count} file(s) with {allContent.Length:N0} characters")
                                .AppendLine($"Time taken: {sw.ElapsedMilliseconds} ms");

                            await shell.ShowPromptAsync(message.ToString(), PromptOptions.OK, cancellationToken);
                        }
                        else
                        {
                            await shell.ShowPromptAsync("An error occurred while reading the files.", PromptOptions.OK, cancellationToken);
                        }
                    }
                    else
                    {
                        await shell.ShowPromptAsync("No .cs files found in the folder.", PromptOptions.OK, cancellationToken);
                    }
                }
                else
                {
                    await shell.ShowPromptAsync("The directory path is null.", PromptOptions.OK, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
                await shell.ShowPromptAsync($"Error extracting folder: {ex.Message}", PromptOptions.OK, cancellationToken);
            }
        }
        else
        {
            await shell.ShowPromptAsync("Please open a valid file first.", PromptOptions.OK, cancellationToken);
        }
    }
}
