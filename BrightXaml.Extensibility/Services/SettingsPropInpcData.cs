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
    public bool UseFieldKeyword { get => useFieldKeyword; set => SetProperty(ref useFieldKeyword, value); }
    private bool useFieldKeyword = false;

    [DataMember]
    public bool UseObservableProperty { get => useObservableProperty; set => SetProperty(ref useObservableProperty, value); }
    private bool useObservableProperty = false;

    [DataMember]
    public bool UseBackingField { get => useBackingField; set => SetProperty(ref useBackingField, value); }
    private bool useBackingField = true;

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
}