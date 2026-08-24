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
    [ObservableProperty] private string slogan = string.Empty;
    [ObservableProperty] private string ville = string.Empty;
    [ObservableProperty] private string pays = string.Empty;
    [ObservableProperty] private string adresse = string.Empty;
    [ObservableProperty] private string telephone = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string nifRccm = string.Empty;
    [ObservableProperty] private string devise = string.Empty;
    [ObservableProperty] private decimal tauxUsdFc = 2850;
    [ObservableProperty] private int seuilAlerteDefaut = 5;
    [ObservableProperty] private string? messageConfirmation;

    public string CheminBaseDeDonnees => AppDbContext.DbPath;
    public string TailleBaseDeDonnees
    {
        get
        {
            try
            {
                if (File.Exists(AppDbContext.DbPath))
                {
                    var info = new FileInfo(AppDbContext.DbPath);
                    return $"{info.Length / 1024.0:F1} Ko";
                }
            }
            catch { }
            return "N/A";
        }
    }

    public ParametresViewModel()
    {
        Charger();
    }

    private void Charger()
    {
        var s = _settingsService.Charger();
        NomEntreprise = s.NomEntreprise;
        Slogan = s.Slogan;
        Ville = s.Ville;
        Pays = s.Pays;
        Adresse = s.Adresse;
        Telephone = s.Telephone;
        Email = s.Email;
        NifRccm = s.NifRccm;
        Devise = s.Devise;
        TauxUsdFc = s.TauxUsdFc;
        SeuilAlerteDefaut = s.SeuilAlerteDefaut;
        OnPropertyChanged(nameof(TailleBaseDeDonnees));
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
            Slogan = Slogan.Trim(),
            Ville = Ville.Trim(),
            Pays = Pays.Trim(),
            Adresse = Adresse.Trim(),
            Telephone = Telephone.Trim(),
            Email = Email.Trim(),
            NifRccm = NifRccm.Trim(),
            Devise = string.IsNullOrWhiteSpace(Devise) ? "FC" : Devise.Trim(),
            TauxUsdFc = TauxUsdFc > 0 ? TauxUsdFc : 2850,
            SeuilAlerteDefaut = SeuilAlerteDefaut > 0 ? SeuilAlerteDefaut : 5
        });

        MessageConfirmation = "Paramètres d'entreprise enregistrés ✓";
        NotificationService.AfficherMessage("✓ Paramètres d'entreprise enregistrés avec succès !");
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
                Filter = "Base de données SQLite (*.db)|*.db",
                FileName = $"kahuzi_erp_sauvegarde_{DateTime.Now:yyyyMMdd_HHmm}.db"
            };
            if (dlg.ShowDialog() == true)
            {
                ExportService.SauvegarderBase(dlg.FileName);
                MessageConfirmation = "Sauvegarde effectuée avec succès ✓";
                NotificationService.AfficherMessage("✓ Base de données sauvegardée avec succès !");
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
                Filter = "Base de données SQLite (*.db)|*.db"
            };
            if (dlg.ShowDialog() == true)
            {
                var rep = System.Windows.MessageBox.Show(
                    "Attention : la restauration écrasera les données actuelles par la sauvegarde sélectionnée.\n\nSouhaitez-vous continuer ?",
                    "Confirmation de restauration",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (rep == System.Windows.MessageBoxResult.Yes)
                {
                    ExportService.RestaurerBase(dlg.FileName);
                    MessageConfirmation = "Base restaurée ✓ Redémarrez l'application pour synchroniser tous les modules.";
                    NotificationService.AfficherMessage("✓ Base restaurée avec succès !");
                    OnPropertyChanged(nameof(TailleBaseDeDonnees));
                }
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Échec de la restauration : {ex.Message}";
        }
    }
}