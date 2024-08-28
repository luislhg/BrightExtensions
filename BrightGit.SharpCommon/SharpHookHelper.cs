using BrightGit.SharpCommon.Models;
using System.Text;

namespace BrightGit.SharpCommon;
public class SharpHookHelper
{
    private const string baseHook = "#!/bin/sh";
    private const string sharpHookFileName = "BrightGit.SharpHook.exe";
    private const string sharpHookCommand = $"exec ./.git/hooks/{sharpHookFileName} PostCheckout \"$(git rev-parse --show-toplevel)\" \"$1\" \"$2\" \"$3\"";
    private const string sharpMigratorFileName = "BrightGit.SharpAutoMigrator.exe";
    private const string sharpMigratorCommand = $"exec ./.git/hooks/{sharpMigratorFileName} \"$1\" \"$2\" \"$3\"";

    /// <summary>
    /// Check if a hook file already exists.
    /// If it doesn't, generate a new hook file.
    /// If it does, append the new hook to the existing hook file.
    /// </summary>
    public static void AddSharpHook(string repoDir, GitHookType hookType = GitHookType.PostCheckout)
    {
        //string hookFilePath = Path.Combine(repoDir, ".git", "hooks", hookType.ToString());
        string hookFilePath = Path.Combine(repoDir, ".git", "hooks", "post-checkout");

        // If the hook file doesn't exist, create a new hook file.
        if (!File.Exists(hookFilePath))
        {
            var sb = new StringBuilder();
            sb.AppendLine(baseHook);
            sb.AppendLine(sharpMigratorCommand);

            File.WriteAllText(hookFilePath, sb.ToString());
        }
        // If the hook file exists, append the sharp command to the existing hook file.
        else
        {
            string hookContent = File.ReadAllText(hookFilePath);
            if (!hookContent.Contains(sharpHookCommand))
            {
                if (!hookContent.EndsWith(Environment.NewLine))
                    hookContent += Environment.NewLine;

                hookContent += sharpHookCommand;
                File.WriteAllText(hookFilePath, hookContent);
            }
        }
    }

    public static bool CheckAutoMigratorHook(string repoDir)
    {
        // Check if the post-checkout hook file exists.
        string hookFilePath = Path.Combine(repoDir, ".git", "hooks", "post-checkout");
        if (File.Exists(hookFilePath))
        {
            string hookContent = File.ReadAllText(hookFilePath);
            if (hookContent.Contains(sharpMigratorCommand))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Add a post-checkout hook to the .git/hooks directory.
    /// </summary>
    public static void AddAutoMigratorHook(string repoDir)
    {
        string hookFilePath = Path.Combine(repoDir, ".git", "hooks", "post-checkout");

        // If the hook file doesn't exist, create a new hook file.
        if (!File.Exists(hookFilePath))
        {
            var sb = new StringBuilder();
            sb.AppendLine(baseHook);
            sb.AppendLine(sharpMigratorCommand);

            File.WriteAllText(hookFilePath, sb.ToString());
        }
        // If the hook file exists, append the migrator command to the existing hook file.
        else
        {
            string hookContent = File.ReadAllText(hookFilePath);
            if (!hookContent.Contains(sharpMigratorCommand))
            {
                if (!hookContent.EndsWith(Environment.NewLine))
                    hookContent += Environment.NewLine;

                hookContent += sharpMigratorCommand;
                File.WriteAllText(hookFilePath, hookContent);
            }
        }

        // TODO: For now we are relying that the exe will already be in the hooks directory.
        //// Extract migrator exe to the .git/hooks directory.
        //string migratorDestination = Path.Combine(hooksDirectory, migratorFileName);
        //File.Copy("AutoMigration.exe", migratorDestination, true);
    }

    /// <summary>
    /// Open the post-checkout hook file and remove the migrator command.
    /// </summary>
    public static void RemoveAutoMigratorHook(string repoDir)
    {
        string hookFilePath = Path.Combine(repoDir, ".git", "hooks", "post-checkout");
        if (File.Exists(hookFilePath))
        {
            // Remove the entire line containing the migrator command.
            string hookContent = File.ReadAllText(hookFilePath);
            hookContent = hookContent.Replace(sharpMigratorCommand, string.Empty);

            // Save the updated hook content.
            File.WriteAllText(hookFilePath, hookContent);
        }
    }

    // TODO: Create events and listen to the pipe and fire events.
}
