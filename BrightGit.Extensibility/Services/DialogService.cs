using BrightGit.Extensibility.Windows;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.Notifications;
using System.Diagnostics;

namespace BrightGit.Extensibility.Services;
public class DialogService : IDialogService
{
    public ShellExtensibility Shell { get; set; }

    public Task ShowDialogProgressAsync(string title, out Action<int, string, bool> progressCallback, CancellationToken cancellationToken)
    {
        title ??= "Please wait...";

        // Create a linked token source so we can cancel the dialog programmatically.
        var dialogCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Show the dialog asynchronously (the caller updates it through the callback while it's open).
        var dialogControl = new ProgressWindowContent();
        dialogControl.ViewModel.ProgressText = title;
        dialogControl.ViewModel.ProgressValue = 0;

        var dialogTask = ShowAndDisposeAsync();

        string lastMessage = title;
        progressCallback = (value, message, completed) =>
        {
            try
            {
                if (dialogControl?.ViewModel != null)
                {
                    lastMessage = message ?? lastMessage;
                    dialogControl.ViewModel.ProgressValue = value;
                    dialogControl.ViewModel.ProgressText = $"{lastMessage} ({value}%)";
                }

                if (completed)
                {
                    // Cancel the dialog token to close it programmatically.
                    dialogCts.Cancel();
                }
            }
            catch (Exception ex)
            {
                // The dialog may already be closed/disposed, updating progress should never crash the operation.
                Debug.WriteLine(ex.Message);
            }
        };

        return dialogTask;

        async Task ShowAndDisposeAsync()
        {
            try
            {
                await Shell.ShowDialogAsync(dialogControl, title, DialogOption.Close, dialogCts.Token);
            }
            finally
            {
                dialogControl.Dispose();
            }
        }
    }

    public async Task<string> ShowPromptOptionsAsync(string message, List<string> items, CancellationToken cancellationToken)
    {
        if (message == null)
            message = "Choose:";

        // Remove duplicates and sort the items.
        items = items.Distinct().ToList();

        // Create a dictionary to map items to unique integers.
        Dictionary<int, string> itemMap = new();
        for (int i = 0; i < items.Count; i++)
        {
            itemMap[i] = items[i];
        }

        // Create PromptOptions and populate it with the item map.
        var promptOptions = new PromptOptions<int>
        {
            DismissedReturns = -1,
            DefaultChoiceIndex = 0,
        };

        foreach (var kvp in itemMap)
        {
            promptOptions.Choices.Add(kvp.Value, kvp.Key);
        }

        // Show the prompt and get the result.
        var result = await Shell.ShowPromptAsync(
            message,
            promptOptions,
            cancellationToken);

        // Map the selected integer back to the corresponding item.
        if (itemMap.ContainsKey(result))
        {
            return itemMap[result];
        }

        // Return null or handle the case when no valid choice is made.
        return null;
    }
}
