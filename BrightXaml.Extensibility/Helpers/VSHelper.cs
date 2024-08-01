using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.ProjectSystem.Query;

namespace BrightXaml.Extensibility.Helpers;
public static class VSHelper
{
    public static async Task<string> GetSolutionPathAsync(WorkspacesExtensibility workspace, CancellationToken cancellationToken)
    {
        // Get the solution path.
        var solutionPath = (await workspace.QuerySolutionAsync(solution => solution.With(p => p.Path), cancellationToken)).FirstOrDefault()?.Path;
        return solutionPath;
    }

    public static async Task<string> GetSolutionNameAsync(WorkspacesExtensibility workspace, CancellationToken cancellationToken)
    {
        return Path.GetFileNameWithoutExtension(await GetSolutionPathAsync(workspace, cancellationToken));
    }

    public static async Task<string> GetSolutionDirectoryAsync(WorkspacesExtensibility workspace, CancellationToken cancellationToken)
    {
        // Get the solution directory.
        var solutionDirectory = (await workspace.QuerySolutionAsync(solution => solution.With(p => p.Directory), cancellationToken)).FirstOrDefault()?.Directory;
        return solutionDirectory;
    }

    public static async Task<List<string>> GetProjectsDirectoryAsync(WorkspacesExtensibility workspace, CancellationToken cancellationToken)
    {
        // Get the directories from all active projects in the solution.
        var projectsDirs = await workspace.QuerySolutionAsync(solution => solution.Get(p => p.Projects).With(p => p.Path), cancellationToken);
        return projectsDirs.Select(p => p.Path).ToList();
    }
}
