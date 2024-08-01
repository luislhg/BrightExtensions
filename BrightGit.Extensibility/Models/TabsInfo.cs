namespace BrightGit.Extensibility.Models;
public class TabsInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string SolutionName { get; set; }
    public string BranchName { get; set; }
    public DateTime DateSaved { get; set; }
    public List<TabDocumentInfo> Tabs { get; set; } = new();
}
