using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightGit.Extensibility.Services;
[DataContract]
public class SettingsData : ObservableObject
{
    [DataMember]
    public SettingsEFCoreData EFCore { get => efcore; set => SetProperty(ref efcore, value); }
    private SettingsEFCoreData efcore;

    [DataMember]
    public SettingsTabsData Tabs { get => tabs; set => SetProperty(ref tabs, value); }
    private SettingsTabsData tabs;

    public SettingsData()
    {
        Tabs = new SettingsTabsData();
        EFCore = new SettingsEFCoreData();
    }
}
