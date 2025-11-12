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
internal class PropertyToINPCAllCommand : Command
{
    private readonly TraceSource logger;
    private readonly SettingsService settingsService;

    public PropertyToINPCAllCommand(TraceSource traceSource, SettingsService settingsService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.settingsService = settingsService;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "(WIP) Property All To INPC")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.MoveProperty, IconSettings.IconAndText),
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorFileName, @"\.(cs)$"),
        TooltipText = "Converts multiple properties to INotifyPropertyChanged properties",
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
        var lineContent = new string(line.Text.ToArray());

        try
        {
            // Convert prop line to INPC.
            var propData = PropToInpcHelper.GetPropertyLineData(lineContent);
            if (propData != null)
            {
                // Try to make an educated guess on the set method name.
                var setMethodName = PropToInpcHelper.GetInUseSetMethodName(textContent) ?? settingsService.Data.PropInpc.SetMethodName;

                // Determine if 'field' keyword should be used (it's already in use in the file or user has asked for it).
                var useFieldKeyword = PropToInpcHelper.GetInUseFieldKeyword(textContent) || settingsService.Data.PropInpc.UseFieldKeyword;

                // Generate INPC property.
                var preserveDefaultValue = settingsService.Data.PropInpc.PreserveDefaultValue;
                var addFieldAbove = settingsService.Data.PropInpc.AddFieldAbove;
                var addFieldUnderscore = settingsService.Data.PropInpc.AddFieldUnderscore;

                // Generate code.
                string generatedCode = (useFieldKeyword)
                    ? PropToInpcHelper.GenerateInpcPropertySetFieldKeyword(propData, preserveDefaultValue, setMethodName)
                    : PropToInpcHelper.GenerateInpcPropertySet(propData, addFieldAbove, addFieldUnderscore, preserveDefaultValue, setMethodName);
                Debug.WriteLine(generatedCode);

                // Apply INPC property.
                await editor.EditAsync(
                batch =>
                {
                    textView.Document.AsEditable(batch).Replace(line.Text, generatedCode);
                },
                cancellationToken);

                sw.Stop();
                Debug.WriteLine($"PropertyToINPCCommand.ExecuteCommandAsync: {sw.ElapsedMilliseconds}ms");
            }
            else
            {
                logger.TraceEvent(TraceEventType.Warning, 0, "No property found in the line.");
            }
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0, ex.Message);
            await shell.ShowPromptAsync($"Error converting property: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }
}
