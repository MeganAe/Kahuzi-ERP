using System.IO;
using System.Text.Json;

namespace GestionCommerciale.Services;

public class AppSettings
{
    public string NomEntreprise { get; set; } = "Kahuzi ERP";
    public string Slogan { get; set; } = "Gestion Commerciale & Progiciel d'Entreprise";
    public string Ville { get; set; } = "Bukavu";
    public string Pays { get; set; } = "RD Congo";
    public string Adresse { get; set; } = "Avenue Patrice Lumumba, Ibanda, Bukavu";
    public string Telephone { get; set; } = "+243 999 000 000";
    public string Email { get; set; } = "contact@kahuzierp.cd";
    public string NifRccm { get; set; } = "RCCM: CD/BKV/2026-B - Id.Nat: 01-G4700";
    public string Devise { get; set; } = "FC";
    public decimal TauxUsdFc { get; set; } = 2850;
    public int SeuilAlerteDefaut { get; set; } = 5;
    public bool ActiverConfirmationSuppression { get; set; } = true;
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