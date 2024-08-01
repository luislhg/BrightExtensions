namespace BrightGit.SharpCommon.Models;
public class RunData
{
    public RunType RunType { get; set; }
    public string RepoDir { get; set; }
    public string[] Parameters { get; set; }
}
