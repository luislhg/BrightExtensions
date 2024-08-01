namespace BrightGit.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility.UI;

/// <summary>
/// A remote user control to use as tool window UI content.
/// </summary>
internal class TabsWindowContent : RemoteUserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TabsWindowContent" /> class.
    /// </summary>
    public TabsWindowContent()
        : base(dataContext: new TabsWindowViewModel())
    {
    }
}
