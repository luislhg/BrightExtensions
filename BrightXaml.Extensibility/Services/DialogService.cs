using BrightXaml.Extensibility.Windows;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.Notifications;

namespace BrightXaml.Extensibility.Services;

public class DialogService : IDialogService
{
    public ShellExtensibility Shell { get; set; }

    public Task<bool> ShowPromptOKAsync(string message, CancellationToken cancellationToken)
    {
        return Shell.ShowPromptAsync(message, PromptOptions.OK, cancellationToken);
    }

    public Task<bool> ShowPromptOKCancelAsync(string message, CancellationToken cancellationToken)
    {
        return Shell.ShowPromptAsync(message, PromptOptions.OKCancel, cancellationToken);
    }

    public Task<bool> ShowPromptRetryCancelAsync(string message, CancellationToken cancellationToken)
    {
        return Shell.ShowPromptAsync(message, PromptOptions.RetryCancel, cancellationToken);
    }

    public Task ShowDialogProgressAsync(string message, out Action<int, bool> progressCallback, CancellationToken cancellationToken)
    {
        message ??= "Please wait...";

        try
        {
            // Create a linked token source so we can cancel the dialog programmatically.
            var dialogCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Show the dialog asynchronously.
            var dialogWindow = new ProgressWindow();
            var dialogControl = dialogWindow.Content;
            dialogControl.ViewModel.ProgressText = message;
            dialogControl.ViewModel.ProgressValue = 0;

            var dialogResult = Shell.ShowDialogAsync(dialogControl, message, DialogOption.Close, dialogCts.Token);
            progressCallback = (value, completed) =>
            {
                if (dialogControl?.ViewModel != null)
                {
                    dialogControl.ViewModel.ProgressValue = (double)value;
                    dialogControl.ViewModel.ProgressText = $"{message} ({value}%)";
                }

                if (completed)
                {
                    // Cancel the dialog token to close it programmatically.
                    dialogCts.Cancel();
                }
            };
        }
        catch (ObjectDisposedException)
        {
            throw new OperationCanceledException();
        }
        catch (Exception)
        {
            throw;
        }

        return Task.CompletedTask;
    }

    public async Task<string> ShowDialogOptionsAsync(string title, string label, List<string> items, CancellationToken cancellationToken)
    {
        if (title == null)
            title = "Choose an item";

        // Remove duplicates and sort the items.
        items = items.Distinct().OrderBy(i => i).ToList();

        // Create a ChooseItemWindowContent instance and populate it with the items.
        var dialogControl = new ChooseItemWindowContent();
        dialogControl.ViewModel.Items = new System.Collections.ObjectModel.ObservableCollection<string>(items);
        dialogControl.ViewModel.SelectedItem = dialogControl.ViewModel.Items.FirstOrDefault();
        dialogControl.ViewModel.LabelText = label;

        // Show the dialog and get the result.
        var dialogResult = await Shell.ShowDialogAsync(dialogControl, title, DialogOption.OKCancel, cancellationToken);

        if (dialogResult == DialogResult.OK)
        {
            return dialogControl.ViewModel.SelectedItem;
        }

        return null;
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
