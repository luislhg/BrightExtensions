namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility.UI;

/// <summary>
/// A remote user control to use as tool window UI content.
/// </summary>
internal class ChooseItemWindowContent : RemoteUserControl
{
    public ChooseItemWindowViewModel ViewModel => base.DataContext as ChooseItemWindowViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChooseItemWindowContent" /> class.
    /// </summary>
    public ChooseItemWindowContent()
        : base(dataContext: new ChooseItemWindowViewModel())
    {
    }
}
