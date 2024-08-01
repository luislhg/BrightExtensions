using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightGit.Extensibility.Services;
[DataContract]
public class SettingsEFCoreData : ObservableObject
{
    [DataMember]
    public bool IsEnabled { get => isEnabled; set => SetProperty(ref isEnabled, value); }
    private bool isEnabled;
}
