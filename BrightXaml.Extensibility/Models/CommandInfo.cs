using System.Runtime.Serialization;

namespace BrightXaml.Extensibility.Models;
[DataContract]
internal class CommandInfo
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Description { get; set; }
}
