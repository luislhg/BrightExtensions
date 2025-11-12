namespace BrightXaml.Extensibility.Windows;

using BrightXaml.Extensibility.Models;
using BrightXaml.Extensibility.Services;
using BrightXaml.Extensibility.Utilities;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;
using System.Text.Json;

[DataContract]
internal class SettingsWindowViewModel : NotifyPropertyChangedObject
{
    public Action<CancellationToken> CloseWindow { get; set; }

    public SettingsService SettingsService { get; }

    [DataMember]
    public SettingsData SettingsData { get => settingsData; private set => SetProperty(ref settingsData, value); }
    private SettingsData settingsData;

    [DataMember]
    public string PreviewInpcInput { get => previewInpcInput; set => SetProperty(ref previewInpcInput, value); }
    private string previewInpcInput;

    [DataMember]
    public string PreviewInpcOutput { get => previewInpcOutput; set => SetProperty(ref previewInpcOutput, value); }
    private string previewInpcOutput;

    [DataMember]
    public List<ComboIntData> TagSpacesOptions { get => tagSpacesOptions; set => SetProperty(ref tagSpacesOptions, value); }
    private List<ComboIntData> tagSpacesOptions;

    [DataMember]
    public AsyncCommand OKCommand { get; }

    [DataMember]
    public AsyncCommand CancelCommand { get; }

    [DataMember]
    public string Version { get; } = Meta.Version.ToString();

    public SettingsWindowViewModel(SettingsService settingsService)
    {
        SettingsService = settingsService;
        SettingsData = CloneSettings(settingsService.Data);

        TagSpacesOptions = new List<ComboIntData>() {
            new (){ Id = -1, Text = "Preserve spaces (0 or 1)" },
            new (){ Id = 0, Text = "Always 0 spaces" },
            new (){ Id = 1, Text = "Always 1 space" },
        };

        OKCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
        {
            // Save settings.
            SaveSettings();

            // Close window.
            CloseWindow?.Invoke(cancellationToken);
            return Task.CompletedTask;
        });
        CancelCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
        {
            // Reset settings.
            SettingsData = CloneSettings(settingsService.Data);
            SettingsData.PropInpc.PropertyChanged += SettingsWindowViewModel_PropertyChanged;

            // Close window.
            CloseWindow?.Invoke(cancellationToken);
            return Task.CompletedTask;
        });

        // Initialize preview data.
        PreviewInpcInput = "public int Age { get; set; }";
        CalculatePreviewInpc();

        // Subscribe to property changes.
        PropertyChanged += SettingsWindowViewModel_PropertyChanged;
        SettingsData.PropInpc.PropertyChanged += SettingsWindowViewModel_PropertyChanged;
    }

    private void SettingsWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsData.PropInpc.AddFieldAbove) ||
            e.PropertyName == nameof(SettingsData.PropInpc.AddFieldUnderscore) ||
            e.PropertyName == nameof(SettingsData.PropInpc.UseFieldKeyword) ||
            e.PropertyName == nameof(SettingsData.PropInpc.SetMethodName))
        {
            CalculatePreviewInpc();
        }
    }

    private void CalculatePreviewInpc()
    {
        var propData = PropToInpcHelper.GetPropertyLineData(PreviewInpcInput);

        if (SettingsData.PropInpc.UseFieldKeyword)
        {
            PreviewInpcOutput = PropToInpcHelper.GenerateInpcPropertySetFieldKeyword(propData,
                                                                                    SettingsData.PropInpc.PreserveDefaultValue,
                                                                                    SettingsData.PropInpc.SetMethodName);
        }
        else
        {
            PreviewInpcOutput = PropToInpcHelper.GenerateInpcPropertySet(propData,
                                                                         SettingsData.PropInpc.AddFieldAbove,
                                                                         SettingsData.PropInpc.AddFieldUnderscore,
                                                                         true,
                                                                         SettingsData.PropInpc.SetMethodName);
        }
    }

    public void SaveSettings()
    {
        SettingsService.Data = SettingsData;
        SettingsService.Save();
    }

    public SettingsData CloneSettings(SettingsData data)
    {
        return JsonSerializer.Deserialize<SettingsData>(JsonSerializer.Serialize(data));
    }
}
