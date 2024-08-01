namespace BrightGit.Extensibility.Windows;

using BrightGit.Extensibility.Services;
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
    public AsyncCommand OKCommand { get; }

    [DataMember]
    public AsyncCommand CancelCommand { get; }

    public SettingsWindowViewModel(SettingsService settingsService)
    {
        SettingsService = settingsService;
        SettingsData = CloneSettings(settingsService.Data);

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

            // Close window.
            CloseWindow?.Invoke(cancellationToken);
            return Task.CompletedTask;
        });
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
