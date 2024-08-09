namespace BrightGit.Extensibility.Windows;

using BrightGit.Extensibility.Services;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A sample tool window.
/// </summary>
[VisualStudioContribution]
public class TabsWindow : ToolWindow
{
    private readonly TabsWindowContent content;

    /// <summary>
    /// Initializes a new instance of the <see cref="TabsWindow" /> class.
    /// </summary>
    public TabsWindow(TabsStorageService tabsStorageService)
    {
        this.Title = "Bright Git - Tabs";
        this.content = new TabsWindowContent(tabsStorageService);
        content.ViewModel.CloseWindow = (cancellationToken) => { _ = HideAsync(cancellationToken); };
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
