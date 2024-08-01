namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

[DataContract]
internal class ProgressWindowViewModel : NotifyPropertyChangedObject
{
    [DataMember]
    public int ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }
    private int _progressValue;

    [DataMember]
    public string ProgressText { get => _progressText; set => SetProperty(ref _progressText, value); }
    private string _progressText;

    public ProgressWindowViewModel()
    {
        ProgressValue = 0;
        ProgressText = "Starting...";
    }
}
