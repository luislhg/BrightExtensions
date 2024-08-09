namespace BrightGit.Extensibility.Windows;

using BrightGit.Extensibility.Services;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;
using System.Text.Json;

[DataContract]
internal class TabsWindowViewModel : NotifyPropertyChangedObject
{
    public Action<CancellationToken> CloseWindow { get; set; }

    [DataMember]
    public TabsStorageData TabsStorageData { get => tabsStorageData; private set => SetProperty(ref tabsStorageData, value); }
    private TabsStorageData tabsStorageData;

    [DataMember]
    public AsyncCommand OKCommand { get; }

    [DataMember]
    public AsyncCommand CancelCommand { get; }

    [DataMember]
    public string Version { get; } = Meta.Version.ToString();

    private readonly TabsStorageService tabsStorageService;

    public TabsWindowViewModel(TabsStorageService tabsStorageService)
    {
        TabsStorageData = CloneTabsStorage(tabsStorageService.Data);

        OKCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
        {
            // Save TabsStorage.
            SaveTabsStorage();

            // Close window.
            CloseWindow?.Invoke(cancellationToken);
            return Task.CompletedTask;
        });
        CancelCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
        {
            // Reset TabsStorage.
            TabsStorageData = CloneTabsStorage(tabsStorageService.Data);

            // Close window.
            CloseWindow?.Invoke(cancellationToken);
            return Task.CompletedTask;
        });

        this.tabsStorageService = tabsStorageService;
    }

    public void SaveTabsStorage()
    {
        tabsStorageService.Data = TabsStorageData;
        tabsStorageService.Save();
    }

    public TabsStorageData CloneTabsStorage(TabsStorageData data)
    {
        return JsonSerializer.Deserialize<TabsStorageData>(JsonSerializer.Serialize(data));
    }
}
