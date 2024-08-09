namespace BrightGit.Extensibility.Windows;

using BrightGit.Extensibility.Services;
using Microsoft.VisualStudio.Extensibility.UI;

/// <summary>
/// A remote user control to use as tool window UI content.
/// </summary>
internal class TabsWindowContent : RemoteUserControl
{
    public TabsWindowViewModel ViewModel => base.DataContext as TabsWindowViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="TabsWindowContent" /> class.
    /// </summary>
    public TabsWindowContent(TabsStorageService tabsStorageService)
        : base(dataContext: new TabsWindowViewModel(tabsStorageService))
    {
    }
}
