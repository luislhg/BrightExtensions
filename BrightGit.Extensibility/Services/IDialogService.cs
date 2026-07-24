using Microsoft.VisualStudio.Extensibility.Shell;

namespace BrightGit.Extensibility.Services;
public interface IDialogService
{
    ShellExtensibility Shell { get; set; }

    /// <summary>
    /// Only use this for a maximum of 5 items... It displays horizontally only.
    /// </summary>
    Task<string> ShowPromptOptionsAsync(string title, List<string> items, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a progress dialog and returns its task (completes when the dialog closes).
    /// Update it through the callback: (percent 0-100, message or null to keep the current one, completed true to close the dialog).
    /// </summary>
    Task ShowDialogProgressAsync(string title, out Action<int, string, bool> progressCallback, CancellationToken cancellationToken);
}