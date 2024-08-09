using System.Diagnostics;
using System.Text.Json;

namespace BrightGit.Extensibility.Services;
public class SettingsService
{
    private const string fileName = "BrightGitSettings.json";
    private readonly string directory;
    private readonly string filePath;

    public SettingsData Data { get; set; } = new();

    public SettingsService()
    {
        directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BrightExtensions");
        filePath = Path.Combine(directory, fileName);
        Load();
    }

    public void Load()
    {
        try
        {
            // Check if directory exists.
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Load settings.
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
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
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            // Add log?
            Debug.WriteLine(ex);
        }
    }
}