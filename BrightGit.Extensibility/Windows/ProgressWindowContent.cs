namespace BrightGit.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility.UI;

/// <summary>
/// A remote user control to use as progress dialog content.
/// </summary>
internal class ProgressWindowContent : RemoteUserControl
{
    public ProgressWindowViewModel ViewModel => base.DataContext as ProgressWindowViewModel;

    public ProgressWindowContent()
        : base(dataContext: new ProgressWindowViewModel())
    {
    }
}
