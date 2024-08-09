using BrightGit.Extensibility.Models;
using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightGit.Extensibility.Services;
[DataContract]
public class TabsStorageData : ObservableObject
{
    [DataMember]
    public List<TabsInfo> TabsBranch { get => tabsBranch; set => SetProperty(ref tabsBranch, value); }
    private List<TabsInfo> tabsBranch = new();

    [DataMember]
    public List<TabsInfo> TabsCustom { get => tabsCustom; set => SetProperty(ref tabsCustom, value); }
    private List<TabsInfo> tabsCustom = new();
}
