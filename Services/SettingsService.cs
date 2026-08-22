using System.IO;
using System.Text.Json;

namespace GestionCommerciale.Services;

public class AppSettings
{
    public string NomEntreprise { get; set; } = "Kahuzi ERP";
    public string Devise { get; set; } = "FC";
}

public class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KahuziERP", "settings.json");

    public AppSettings Charger()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings is not null) return settings;
            }
        }
        catch
        {
            // Si le fichier est corrompu, on retombe sur les valeurs par défaut.
        }
        return new AppSettings();
    }

    public void Enregistrer(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
        DataChangeNotifier.Notify();
    }
}