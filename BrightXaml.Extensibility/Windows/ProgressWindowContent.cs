namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility.UI;

/// <summary>
/// A remote user control to use as tool window UI content.
/// </summary>
internal class ProgressWindowContent : RemoteUserControl
{
    public ProgressWindowViewModel ViewModel => base.DataContext as ProgressWindowViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressWindowContent" /> class.
    /// </summary>
    public ProgressWindowContent()
        : base(dataContext: new ProgressWindowViewModel())
    {
    }
}
