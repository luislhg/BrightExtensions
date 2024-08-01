using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.Notifications;

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

    Task<DialogResult> ShowDialogProgressAsync(string message, out Action<int> progress, CancellationToken cancellationToken);
}