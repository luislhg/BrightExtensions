using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightXaml.Extensibility.Services;
[DataContract]
public class SettingsGoToBindingData : ObservableObject
{
    [DataMember]
    public bool IsEnabled { get => isEnabled; set => SetProperty(ref isEnabled, value); }
    private bool isEnabled = true;
}
