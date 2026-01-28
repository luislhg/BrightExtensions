namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

[DataContract]
internal class ProgressWindowViewModel : NotifyPropertyChangedObject
{
    public Action<CancellationToken> CloseWindow { get; set; }

    [DataMember]
    public double ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }
    private double _progressValue;

    [DataMember]
    public string ProgressText { get => _progressText; set => SetProperty(ref _progressText, value); }
    private string _progressText;

    public ProgressWindowViewModel()
    {
        ProgressValue = 0;
        ProgressText = "Please wait...";
    }
}
