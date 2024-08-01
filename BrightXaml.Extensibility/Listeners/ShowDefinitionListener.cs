﻿using BrightXaml.Extensibility.Services;
using BrightXaml.Extensibility.Utilities;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.ProjectSystem.Query;
using Microsoft.VisualStudio.RpcContracts.Documents;
using Microsoft.VisualStudio.RpcContracts.OpenDocument;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Range = Microsoft.VisualStudio.RpcContracts.Utilities.Range;

namespace BrightXaml.Extensibility.Listeners;
[VisualStudioContribution]
public class ShowDefinitionListener : ExtensionPart, ITextViewOpenClosedListener
{
    private readonly TraceSource traceSource;
    private readonly SettingsService settingsService;

    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromGlobPattern("**/*.cs", true),
        ],
    };

    public ShowDefinitionListener(TraceSource traceSource, SettingsService settingsService)
    {
        this.traceSource = traceSource;
        this.settingsService = settingsService;
    }

    public Task TextViewClosedAsync(ITextViewSnapshot textView, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task TextViewOpenedAsync(ITextViewSnapshot textView, CancellationToken cancellationToken)
    {
        // Check if the feature is enabled.
        if (!settingsService.Data.GoToBinding.IsEnabled)
            return;

        // Check if it's a generated document (MVVM Toolkit Source Generator)
        if (textView?.Document?.Uri?.Segments?.Contains("VSGeneratedDocuments/") == true)
        {
            try
            {
                Debug.WriteLine("TextViewOpenedAsync: VSGeneratedDocuments");
                var sw = Stopwatch.StartNew();

                // Get the text content of the document.
                var textContent = new string(textView.Document.Text.ToArray());

                // Extract the class name from the document.
                Match match = Regex.Match(textContent, @"\bpartial class\s+(\w+)\b");
                if (match.Success)
                {
                    var className = match.Groups[1].Value;
                    Debug.WriteLine($"Class name: {className}");

                    // Check if the line is from a command.
                    if (textContent.Contains("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator"))
                    {
                        Debug.WriteLine("TextViewOpenedAsync: RelayCommandGenerator");

                        // Extract the method name.
                        var methodName = textView.Document.Uri.Segments.LastOrDefault().Replace(".g.cs", string.Empty).Split(".").LastOrDefault();
                        Debug.WriteLine($"Method name: {methodName}");

                        // Find the file in the project.
                        var viewModelBadPath = textView.Document.Uri.Segments.LastOrDefault().Replace($".{methodName}.g.cs", string.Empty);
                        viewModelBadPath = viewModelBadPath.Replace(".", "\\") + ".cs";
                        var viewModelFileName = Path.GetFileName(viewModelBadPath);
                        var files = await Extensibility.Workspaces()
                                        .QueryProjectsAsync(project => project
                                            .Get(p => p.Files)
                                            .Where(f => f.Path.EndsWith(viewModelFileName, StringComparison.InvariantCultureIgnoreCase))
                                            .With(f => new { f.FileName, f.Path }), cancellationToken);

                        var result = files.FirstOrDefault().Path;
                        Debug.WriteLine($"ViewModel path: {result} ({sw.ElapsedMilliseconds}ms)");

                        // Open the file where the binding is defined.
                        var bindingWordOffset = ShowDefinitionHelper.GetRelayCommandOffset_MVVMToolkit(methodName, result);
                        var bindingWordLine = ShowDefinitionHelper.GetRelayCommandLine_MVVMToolkit(methodName, result);
                        var bindingStartOffsetFromLine = ShowDefinitionHelper.GetRelayCommandOffsetFromLine_MVVMToolkit(methodName, result);
                        var bindingWordEndOffsetFromLine = bindingStartOffsetFromLine + methodName.Length;
                        await OpenFileAsync(result, bindingWordLine, bindingWordEndOffsetFromLine, cancellationToken);

                        // Close current document.
                        await Extensibility.Documents().CloseDocumentAsync(textView.Document.Uri, SaveDocumentOption.NoSave, cancellationToken);
                    }
                    // Check if the line is from a property.
                    else if (textContent.Contains("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator"))
                    {
                        Debug.WriteLine("TextViewOpenedAsync: ObservablePropertyGenerator");

                        // TODO: API BUG?
                        // Offsets only get a value at the first run, after they always have 0, thus this is not working as expected.
                        Debug.WriteLine($"ActivePosition {textView.Selection.ActivePosition.Offset}");
                        Debug.WriteLine($"InsertionPosition {textView.Selection.InsertionPosition.Offset}");
                        Debug.WriteLine($"AnchorPosition {textView.Selection.AnchorPosition.Offset}");

                        // Extract the method or property name from the line, starting at the caret position and ending with a whitespace or breakline.
                        var line = textView.Document.GetLineFromPosition(textView.Selection.InsertionPosition);
                        var lineContent = new string(line.Text.ToArray());
                        var insertionPositionOffset = textView.Selection.InsertionPosition.Offset;
                        var lineStartOffset = textContent.LastIndexOf('\n', insertionPositionOffset - 1) + 1;
                        if (lineStartOffset == 0 && insertionPositionOffset == 0)
                        {
                            lineStartOffset = 0;
                        }
                        var lineCaretOffset = insertionPositionOffset - lineStartOffset;
                        var bindingWord = ShowDefinitionHelper.GetWordAtCaret(lineContent, lineCaretOffset);
                        Debug.WriteLine($"Binding word: {bindingWord}");

                        // Extract the field name.
                        var textPartial = textContent.Substring(insertionPositionOffset);
                        const string textFind = "get => ";
                        var fieldStartIndex = textPartial.IndexOf(textFind) + textFind.Length;
                        var fieldEndIndex = textPartial.IndexOf(';', fieldStartIndex);
                        string fieldName = textPartial.Substring(fieldStartIndex, fieldEndIndex - fieldStartIndex).Trim();
                        Debug.WriteLine($"Field name: {fieldName}");

                        // Find the file in the project.
                        var viewModelBadPath = textView.Document.Uri.Segments.LastOrDefault().Replace($".g.cs", string.Empty);
                        viewModelBadPath = viewModelBadPath.Replace(".", "\\") + ".cs";
                        var viewModelFileName = Path.GetFileName(viewModelBadPath);
                        var files = await Extensibility.Workspaces()
                                        .QueryProjectsAsync(project => project
                                            .Get(p => p.Files)
                                            .Where(f => f.Path.EndsWith(viewModelFileName, StringComparison.InvariantCultureIgnoreCase))
                                            .With(f => new { f.FileName, f.Path }), cancellationToken);

                        var result = files.FirstOrDefault().Path;
                        Debug.WriteLine($"ViewModel path: {result} ({sw.ElapsedMilliseconds}ms)");

                        // Open the file where the binding is defined.
                        var bindingWordOffset = ShowDefinitionHelper.GetObservablePropertyOffset_MVVMToolkit(fieldName, result);
                        var bindingWordLine = ShowDefinitionHelper.GetObservablePropertyLine_MVVMToolkit(fieldName, result);
                        var bindingStartOffsetFromLine = ShowDefinitionHelper.GetRelayCommandOffsetFromLine_MVVMToolkit(fieldName, result);
                        var bindingWordEndOffsetFromLine = bindingStartOffsetFromLine + fieldName.Length;
                        await OpenFileAsync(result, bindingWordLine, bindingWordEndOffsetFromLine, cancellationToken);

                        // Close current document.
                        await Extensibility.Documents().CloseDocumentAsync(textView.Document.Uri, SaveDocumentOption.NoSave, cancellationToken);
                    }
                    else
                    {
                        Debug.WriteLine("Source Generator type not recognized.");
                        traceSource.TraceEvent(TraceEventType.Warning, 0, "Source Generator type not recognized.");
                    }
                }
                else
                {
                    Debug.WriteLine("Class name not found.");
                    traceSource.TraceEvent(TraceEventType.Warning, 0, "Class name not found.");
                }

                sw.Stop();
                Debug.WriteLine($"ShowDefinitionListener ({sw.ElapsedMilliseconds}ms)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                traceSource.TraceEvent(TraceEventType.Error, 0, ex.ToString());
            }
        }
    }

    public async Task OpenFileAsync(string fileName, int lineStart, int columnStartOffset, CancellationToken cancellationToken)
    {
        var options = new OpenDocumentOptions(activate: true, selection: new Range(lineStart, columnStartOffset, 0, 0));
        //var options = new OpenDocumentOptions(activate: true, ensureVisible: new Range(caretLine, 0, 0, 0), ensureVisibleOptions: EnsureRangeVisibleOptions.MinimumScroll);
        await Extensibility.Documents().OpenTextDocumentAsync(new Uri(fileName), options, cancellationToken);
    }
}
