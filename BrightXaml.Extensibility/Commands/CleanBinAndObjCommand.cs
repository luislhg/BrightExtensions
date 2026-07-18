namespace BrightXaml.Extensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.ProjectSystem.Query;
using Microsoft.VisualStudio.RpcContracts.ProgressReporting;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
internal class CleanBinAndObjCommand : Command
{
    private readonly TraceSource logger;

    public CleanBinAndObjCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Clean Solution Bin and Obj")
    {
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.CleanData, IconSettings.IconAndText),
        TooltipText = "Cleans bin and obj directories for all projects in the solution"
    };

    /// <inheritdoc />
    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Use InitializeAsync for any one-time setup or initialization.
        return base.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var shell = Extensibility.Shell();
        var workspace = Extensibility.Workspaces();
        var documents = Extensibility.Documents();

        try
        {
            // Get the directories from all active projects in the solution.
            var projectsDirs = (await workspace.QuerySolutionAsync(solution => solution.Get(p => p.Projects).With(p => p.Path), cancellationToken)).ToList();
            var projectsCleaned = new StringBuilder();
            int i = 0, max = projectsDirs.Count;
            using (var progress = await shell.StartProgressReportingAsync("Cleaning bin and obj directories", new(true), cancellationToken))
            {
                foreach (var project in projectsDirs)
                {
                    var projectDirectory = Path.GetDirectoryName(project.Path);
                    if (!string.IsNullOrWhiteSpace(projectDirectory))
                    {
                        Debug.WriteLine($"Cleaning bin and obj directories in {projectDirectory}");

                        var projectName = Path.GetFileName(projectDirectory);
                        progress.Report(CreateProgressStatus(i++, max, $"Cleaning bin and obj for: {projectName}"));
                        progress.CancellationToken.ThrowIfCancellationRequested();
                        await (Task.Delay(2000));
                        await CleanDirectoryAsync(projectDirectory, cancellationToken);
                        projectsCleaned.AppendLine(projectName);
                    }
                }
            }

            sw.Stop();
            await shell.ShowPromptAsync($"Cleaned bin and obj directories for {projectsDirs.Count} projects:" +
                                        $"\n{projectsCleaned}" +
                                        $"\nTime Elapsed: {sw.Elapsed.TotalMilliseconds:N0}ms",
                                        PromptOptions.OK, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await shell.ShowPromptAsync("Operation canceled by user.", PromptOptions.OK, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.TraceEvent(TraceEventType.Error, 0, $"Error in CleanBinAndObjCommand: {ex}");
            await shell.ShowPromptAsync($"An error occurred: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }

    private async Task CleanDirectoryAsync(string directoryPath, CancellationToken cancellationToken)
    {
        if (directoryPath == null)
            return;

        var optionsTargetSubdirectories = new string[] { "bin", "obj" };

        try
        {
            foreach (var di in optionsTargetSubdirectories.Select(x => Path.Combine(directoryPath, x))
                .Where(Directory.Exists)
                .Select(x => new DirectoryInfo(x)))
            {
                foreach (var file in di.EnumerateFiles())
                {
                    file.Delete();
                }

                foreach (var dir in di.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            logger.TraceEvent(TraceEventType.Error, 0, ex.Message);
            await Extensibility.Shell().ShowPromptAsync($"Error while cleaning directory {directoryPath}: {ex.Message}", PromptOptions.OK, cancellationToken);
        }
    }

    public static ProgressStatus CreateProgressStatus(int current, int max, string msg = "Please wait...") => new((int)(current / (double)max * 100), msg);
}
