using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightGit.Extensibility.Models;
[DataContract]
public class TabsInfo : ObservableObject
{
    [DataMember]
    public string Id { get; set; }

    [DataMember]
    public string SolutionName { get; set; }

    [DataMember]
    public string BranchName { get; set; }

    [DataMember]
    public string Name { get => name; set => SetProperty(ref name, value); }
    private string name;

    [DataMember]
    public DateTime DateSaved { get => dateSaved; set => SetProperty(ref dateSaved, value); }
    private DateTime dateSaved;

    [DataMember]
    public List<TabDocumentInfo> Tabs { get; set; } = new();
}
