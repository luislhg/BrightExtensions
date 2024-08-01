using Microsoft.VisualStudio.PlatformUI;
using System.Runtime.Serialization;

namespace BrightXaml.Extensibility.Services;
[DataContract]
public class SettingsData : ObservableObject
{
    [DataMember]
    public SettingsPropInpcData PropInpc { get => propInpc; set => SetProperty(ref propInpc, value); }
    private SettingsPropInpcData propInpc;

    [DataMember]
    public SettingsFormatXamlData FormatXaml { get => formatXaml; set => SetProperty(ref formatXaml, value); }
    private SettingsFormatXamlData formatXaml;

    [DataMember]
    public SettingsGoToBindingData GoToBinding { get => goToBinding; set => SetProperty(ref goToBinding, value); }
    private SettingsGoToBindingData goToBinding;

    public SettingsData()
    {
        PropInpc = new SettingsPropInpcData();
        FormatXaml = new SettingsFormatXamlData();
        GoToBinding = new SettingsGoToBindingData();
    }
}
