namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A sample tool window.
/// </summary>
[VisualStudioContribution]
public class HelpWindow : ToolWindow
{
    private readonly HelpWindowContent content = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpWindow" /> class.
    /// </summary>
    public HelpWindow()
    {
        this.Title = "Bright Xaml - Help";
    }

    /// <inheritdoc />
    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        // Use this object initializer to set optional parameters for the tool window.
        Placement = ToolWindowPlacement.Floating,
    };

    /// <inheritdoc />
    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Use InitializeAsync for any one-time setup or initialization.
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IRemoteUserControl>(content);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            content.Dispose();

        base.Dispose(disposing);
    }
}
