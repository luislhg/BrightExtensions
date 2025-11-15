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
internal class PropertyToINPCCommand : Command
{
    private readonly TraceSource logger;
    private readonly SettingsService settingsService;

    public PropertyToINPCCommand(TraceSource traceSource, SettingsService settingsService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.settingsService = settingsService;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Property To INPC (Ctrl+E+P)")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.MoveProperty, IconSettings.IconAndText),
        Shortcuts = [new CommandShortcutConfiguration(ModifierKey.Control, Key.E, ModifierKey.Control, Key.P)],
        EnabledWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorFileName, @"\.(cs)$"),
        TooltipText = "Converts a property to an INotifyPropertyChanged property",
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

                // 1) Detect what is actually in use in the file.
                bool inUseFieldKeyword = PropToInpcHelper.GetInUseFieldKeyword(textContent);
                bool inUseObservableProperty = PropToInpcHelper.GetInUseObservableProperty(textContent);
                var sw1 = Stopwatch.StartNew();
                bool inUseBackingField = PropToInpcHelper.GetInUseBackingField(textContent, setMethodName);
                sw1.Stop();
                Debug.WriteLine($"GetInUseBackingField: {sw1.ElapsedMilliseconds}ms");

                // 2) Decide what to use, respecting priority: FieldKeyword > ObservableProperty > BackingField.
                bool useFieldKeyword = false;
                bool useObservableProperty = false;
                bool useBackingField = false;

                if (inUseFieldKeyword || inUseObservableProperty || inUseBackingField)
                {
                    // There is at least one pattern in the file; follow the file.
                    if (inUseFieldKeyword)
                        useFieldKeyword = true;
                    else if (inUseObservableProperty)
                        useObservableProperty = true;
                    else if (inUseBackingField)
                        useBackingField = true;
                }
                else
                {
                    // No pattern found in the file; fallback to user preferences.
                    if (settingsService.Data.PropInpc.UseFieldKeyword)
                        useFieldKeyword = true;
                    else if (settingsService.Data.PropInpc.UseObservableProperty)
                        useObservableProperty = true;
                    else
                        useBackingField = true;
                }

                // Generate INPC property.
                var preserveDefaultValue = settingsService.Data.PropInpc.PreserveDefaultValue;
                var addFieldAbove = settingsService.Data.PropInpc.AddFieldAbove;
                var addFieldUnderscore = settingsService.Data.PropInpc.AddFieldUnderscore;

                // Generate code.
                string generatedCode = string.Empty;
                if (useFieldKeyword)
                    generatedCode = PropToInpcHelper.GenerateInpcPropertySetFieldKeyword(propData, preserveDefaultValue, setMethodName);
                else if (useObservableProperty)
                    generatedCode = PropToInpcHelper.GenerateInpcPropertyObservableProperty(propData, preserveDefaultValue);
                else
                    generatedCode = PropToInpcHelper.GenerateInpcPropertySet(propData, addFieldAbove, addFieldUnderscore, preserveDefaultValue, setMethodName);

                // For debugging.
                Debug.WriteLine($"UseFieldKeyword: {useFieldKeyword}" +
                                $", UseObservableProperty: {useObservableProperty}" +
                                $", UseBackingField: {useBackingField}");
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
