using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightXaml.Extensibility.Services;
[DataContract]
public class SettingsPropInpcData : ObservableObject
{
    [DataMember]
    public bool IsEnabled { get => isEnabled; set => SetProperty(ref isEnabled, value); }
    private bool isEnabled = true;

    [DataMember]
    public bool AddFieldAbove { get => addFieldAbove; set => SetProperty(ref addFieldAbove, value); }
    private bool addFieldAbove = false;

    [DataMember]
    public bool AddFieldUnderscore { get => addFieldUnderscore; set => SetProperty(ref addFieldUnderscore, value); }
    private bool addFieldUnderscore = false;

    [DataMember]
    public string SetMethodName { get => setMethodName; set => SetProperty(ref setMethodName, value); }
    private string setMethodName = "SetProperty";

    [DataMember]
    public bool PreserveDefaultValue { get => preserveDefaultValue; set => SetProperty(ref preserveDefaultValue, value); }
    private bool preserveDefaultValue = true;

    [DataMember]
    public bool SetTryAutoMethodName { get => setTryAutoMethodName; set => SetProperty(ref setTryAutoMethodName, value); }
    private bool setTryAutoMethodName = true;
}
