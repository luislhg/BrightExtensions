using BrightGit.SharpCommon.Models;
using System.Diagnostics;

namespace BrightGit.SharpCommon;
public class SharpRunHelper
{
    public Action<string> ActionMessageReceived { get; set; }

    private const string sharpRunFileName = "BrightGit.SharpRun.exe";

    public void ExecuteGitAction(RunData data)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = sharpRunFileName,
            Arguments = $"{data.RunType} {data.RepoDir} {string.Join(" ", data.Parameters)}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        {
            using (var reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();

                // Raise event to notify the caller.
                ActionMessageReceived?.Invoke(result);
            }
        }
    }
}
