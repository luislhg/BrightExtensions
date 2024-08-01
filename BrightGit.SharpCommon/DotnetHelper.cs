using System.Diagnostics;

namespace BrightGit.SharpCommon;
public static class DotnetHelper
{
    public static Task<bool> UpdateDatabaseEFCoreAsync(string projectDir, string migrationName)
    {
        return RunDotnetCommandAsync(projectDir, $"ef database update {migrationName}");
    }

    public static async Task<bool> RunDotnetCommandAsync(string projectDir, string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = projectDir
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); Debug.WriteLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); Debug.WriteLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
    }

    public static async Task<bool> CheckDotnetToolInstalledAsync(string toolName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "tool list --global",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            await process.WaitForExitAsync();

            // Check if the toolName is in the output
            return output.Contains(toolName, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static Task<bool> CheckDotnetT4Installed()
    {
        return CheckDotnetToolInstalledAsync("dotnet-t4");
    }

    public static Task<bool> CheckDotnetEFInstalled()
    {
        return CheckDotnetToolInstalledAsync("dotnet-ef");
    }

    public static void BuildSolutionEFCore(string solutionPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            process.WaitForExit();
        }
    }
}
