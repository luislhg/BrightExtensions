using System.Diagnostics;
using System.Text.Json;

namespace BrightXaml.Extensibility.Services;
public class SettingsService
{
    private const string settingsFileName = "BrightXamlSettings.json";
    private readonly string settingsDirectory;
    private readonly string settingsFilePath;

    public SettingsData Data { get; set; } = new();

    public SettingsService()
    {
        settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BrightExtensions");
        settingsFilePath = Path.Combine(settingsDirectory, settingsFileName);
        Load();
    }

    public void Load()
    {
        try
        {
            // Check if directory exists.
            if (!Directory.Exists(settingsDirectory))
                Directory.CreateDirectory(settingsDirectory);

            // Load settings.
            if (File.Exists(settingsFilePath))
            {
                var json = File.ReadAllText(settingsFilePath);
                Data = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            // Use default settings.
            Data = new SettingsData();
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }
        catch (Exception ex)
        {
            // Add log?
            Debug.WriteLine(ex);
        }
    }
}