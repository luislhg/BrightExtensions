using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightXaml.Extensibility.Services;
[DataContract]
public class SettingsFormatXamlData : ObservableObject
{
    [DataMember]
    public bool RemoveEmptyLines { get => removeEmptyLines; set => SetProperty(ref removeEmptyLines, value); }
    private bool removeEmptyLines = true;

    [DataMember]
    public int ClosingTagSpaces { get => closingTagSpaces; set => SetProperty(ref closingTagSpaces, value); }
    private int closingTagSpaces = -1;

    [DataMember]
    public int EndingTagSpaces { get => endingTagSpaces; set => SetProperty(ref endingTagSpaces, value); }
    private int endingTagSpaces = -1;

    [DataMember]
    public bool? IndentWithHeader { get => indentWithHeader; set => SetProperty(ref indentWithHeader, value); }
    private bool? indentWithHeader = null;
}
