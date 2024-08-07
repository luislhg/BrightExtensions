using System.Runtime.Serialization;

namespace BrightGit.Extensibility.Models;
[DataContract]
public class TabDocumentInfo
{
    [DataMember]
    public string FilePath { get; set; }

    [DataMember]
    public int Index { get; set; }

    [DataMember]
    public bool IsPinned { get; set; }

    [DataMember]
    public string FileName => (!string.IsNullOrWhiteSpace(FilePath)) ? Path.GetFileName(FilePath) : string.Empty;
}
