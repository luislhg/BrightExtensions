using Microsoft.VisualStudio.Extensibility.Shell;

namespace BrightXaml.Extensibility.Services;

public interface IDialogService
{
    ShellExtensibility Shell { get; set; }

    Task<string> ShowDialogOptionsAsync(string title, string label, List<string> items, CancellationToken cancellationToken);

    Task<bool> ShowPromptOKAsync(string message, CancellationToken cancellationToken);
    Task<bool> ShowPromptOKCancelAsync(string message, CancellationToken cancellationToken);
    Task<bool> ShowPromptRetryCancelAsync(string message, CancellationToken cancellationToken);

    /// <summary>
    /// Only use this for a maximum of 5 items... It displays horizontally only.
    /// </summary>
    Task<string> ShowPromptOptionsAsync(string title, List<string> items, CancellationToken cancellationToken);

    /// <summary>
    /// Display a dialog with a dynamic progress bar and text.
    /// </summary>
    /// <param name="message">Message to be displayed</param>
    /// <param name="progressCallback">Percent complete from 0 to 100 (Progress Bar)</param>
    /// <param name="cancellationToken">Used to inform when the operation has been completed</param>
    /// <returns></returns>
    Task ShowDialogProgressAsync(string message, out Action<int, bool> progressCallback, CancellationToken cancellationToken);
}