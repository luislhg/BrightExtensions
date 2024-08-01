namespace BrightGit.Extensibility.Windows;

using BrightGit.Extensibility.Services;
using Microsoft.VisualStudio.Extensibility.UI;

/// <summary>
/// A remote user control to use as tool window UI content.
/// </summary>
internal class SettingsWindowContent : RemoteUserControl
{
    public SettingsWindowViewModel ViewModel => base.DataContext as SettingsWindowViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsWindowContent" /> class.
    /// </summary>
    public SettingsWindowContent(SettingsService settingsService)
        : base(dataContext: new SettingsWindowViewModel(settingsService))
    {
    }
}
