using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightXaml.Extensibility.Models;
[DataContract]
public class ComboIntData : ObservableObject
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public string Text { get; set; }
}
