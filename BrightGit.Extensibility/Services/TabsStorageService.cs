using BrightGit.Extensibility.Models;
using System.Diagnostics;
using System.Text.Json;

namespace BrightGit.Extensibility.Services;
public class TabsStorageService
{
    private const string fileName = "BrightTabs.json";
    private readonly string fileDirectory;
    private readonly string filePath;

    public TabsStorageData Data { get; set; } = new();

    public TabsStorageService()
    {
        fileDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BrightExtensions");
        filePath = Path.Combine(fileDirectory, fileName);
        Load();
    }

    public void Load()
    {
        try
        {
            // Check if directory exists.
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            // Load settings.
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                Data = JsonSerializer.Deserialize<TabsStorageData>(json) ?? new TabsStorageData();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            // Use default settings.
            Data = new TabsStorageData();
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            // Add log?
            Debug.WriteLine(ex);
        }
    }

    public void AddTabsCustom(TabsInfo tabsInfo)
    {
        AddTabs(tabsInfo, Data.TabsCustom);
    }

    public void AddTabsBranch(TabsInfo tabsInfo)
    {
        AddTabs(tabsInfo, Data.TabsBranch);
    }

    private void AddTabs(TabsInfo tabsInfo, List<TabsInfo> tabs)
    {
        if (tabsInfo != null)
        {
            // Remove existing tabs info if any.
            var existingTabsInfo = tabs.FirstOrDefault(x => x.Id == tabsInfo.Id);
            if (existingTabsInfo != null)
                tabs.Remove(existingTabsInfo);

            // Add new tabs info.
            tabs.Add(tabsInfo);
        }
    }
}
