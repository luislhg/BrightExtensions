namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using System.Threading;
using System.Threading.Tasks;

[VisualStudioContribution]
public class ProgressWindow : ToolWindow
{
    internal ProgressWindowContent Content { get; private set; }
    internal ProgressWindowViewModel ViewModel { get; private set; }

    public ProgressWindow()
    {
        this.Title = "Progress Window";
        this.Content = new ProgressWindowContent();
        this.ViewModel = Content.ViewModel;
        Content.ViewModel.CloseWindow = (cancellationToken) => { _ = HideAsync(cancellationToken); };
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
        return Task.FromResult<IRemoteUserControl>(Content);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Content.Dispose();

        base.Dispose(disposing);
    }
}
