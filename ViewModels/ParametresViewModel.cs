using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Data;
using GestionCommerciale.Services;
using Microsoft.Win32;

namespace GestionCommerciale.ViewModels;

public partial class ParametresViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService = new();

    [ObservableProperty] private string nomEntreprise = string.Empty;
    [ObservableProperty] private string devise = string.Empty;
    [ObservableProperty] private string? messageConfirmation;

    public string CheminBaseDeDonnees => AppDbContext.DbPath;

    public ParametresViewModel()
    {
        Charger();
    }

    private void Charger()
    {
        var settings = _settingsService.Charger();
        NomEntreprise = settings.NomEntreprise;
        Devise = settings.Devise;
    }

    [RelayCommand]
    private void Enregistrer()
    {
        MessageErreur = null;
        MessageConfirmation = null;

        if (string.IsNullOrWhiteSpace(NomEntreprise))
        {
            MessageErreur = "Le nom de l'entreprise est obligatoire.";
            return;
        }

        _settingsService.Enregistrer(new AppSettings
        {
            NomEntreprise = NomEntreprise.Trim(),
            Devise = string.IsNullOrWhiteSpace(Devise) ? "FC" : Devise.Trim()
        });

        MessageConfirmation = "Paramètres enregistrés ✓";
    }

    [RelayCommand]
    private void SauvegarderBase()
    {
        MessageErreur = null;
        MessageConfirmation = null;
        try
        {
            var dlg = new SaveFileDialog
            {
                Title = "Sauvegarder la base de données",
                Filter = "Base de données (*.db)|*.db",
                FileName = $"kahuzi_erp_backup_{DateTime.Now:yyyyMMdd_HHmm}.db"
            };
            if (dlg.ShowDialog() == true)
            {
                File.Copy(AppDbContext.DbPath, dlg.FileName, overwrite: true);
                MessageConfirmation = "Sauvegarde effectuée avec succès ✓";
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Échec de la sauvegarde : {ex.Message}";
        }
    }

    [RelayCommand]
    private void RestaurerBase()
    {
        MessageErreur = null;
        MessageConfirmation = null;
        try
        {
            var dlg = new OpenFileDialog
            {
                Title = "Restaurer une sauvegarde",
                Filter = "Base de données (*.db)|*.db"
            };
            if (dlg.ShowDialog() == true)
            {
                File.Copy(dlg.FileName, AppDbContext.DbPath, overwrite: true);
                MessageConfirmation = "Base restaurée. Redémarrez l'application pour appliquer les changements.";
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Échec de la restauration : {ex.Message}";
        }
    }
}