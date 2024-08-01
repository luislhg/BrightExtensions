namespace BrightXaml.Extensibility.Models;
public class BindingDefinitionOffsets
{
    public string BindingWord { get; set; }

    public int Line { get; set; }
    public int OffsetFromFile { get; set; }
    public int OffsetFromLine { get; set; }

    //public int AttributeLine { get; set; }
    //public int TargetLine { get; set; }
}
