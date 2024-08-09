using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightGit.Extensibility.Services;
[DataContract]
public class SettingsTabsData : ObservableObject
{
    [DataMember]
    public bool IsEnabled { get => isEnabled; set => SetProperty(ref isEnabled, value); }
    private bool isEnabled;

    [DataMember]
    public bool CloseTabsOnSave { get => closeTabsOnSave; set => SetProperty(ref closeTabsOnSave, value); }
    private bool closeTabsOnSave;

    [DataMember]
    public bool CloseTabsOnBranchChange { get => closeTabsOnBranchChange; set => SetProperty(ref closeTabsOnBranchChange, value); }
    private bool closeTabsOnBranchChange = true;
}
