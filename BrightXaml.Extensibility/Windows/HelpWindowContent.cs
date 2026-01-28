namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility.UI;

/// <summary>
/// A remote user control to use as tool window UI content.
/// </summary>
internal class HelpWindowContent : RemoteUserControl
{
    public HelpWindowContent()
        : base(dataContext: new HelpWindowViewModel())
    {
    }
}
