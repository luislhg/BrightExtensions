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
public class SettingsWindow : ToolWindow
{
    private readonly SettingsWindowContent content;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsWindow" /> class.
    /// </summary>
    public SettingsWindow(SettingsService settingsService)
    {
        this.Title = "Bright Git - Settings";
        this.content = new SettingsWindowContent(settingsService);
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

    public override Task OnShowAsync(CancellationToken cancellationToken)
    {
        return base.OnShowAsync(cancellationToken);
    }

    public override Task OnHideAsync(CancellationToken cancellationToken)
    {
        // Auto save when closing?
        //content.ViewModel.SaveSettings();
        return base.OnHideAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            content.Dispose();

        base.Dispose(disposing);
    }
}
