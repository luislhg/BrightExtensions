using Microsoft.VisualStudio.Extensibility.Shell;

namespace BrightGit.Extensibility.Services;
public interface IDialogService
{
    ShellExtensibility Shell { get; set; }

    /// <summary>
    /// Only use this for a maximum of 5 items... It displays horizontally only.
    /// </summary>
    Task<string> ShowPromptOptionsAsync(string title, List<string> items, CancellationToken cancellationToken);
}