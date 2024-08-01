using Microsoft.VisualStudio.Extensibility.Shell;

namespace BrightGit.Extensibility.Services;
public class DialogService : IDialogService
{
    public ShellExtensibility Shell { get; set; }

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
