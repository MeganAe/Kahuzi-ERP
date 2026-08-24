using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Models;
using GestionCommerciale.Services;

namespace GestionCommerciale.ViewModels;

public partial class StockViewModel : ViewModelBase
{
    private readonly ProduitService _produitService = new();
    private List<Produit> _tousLesProduits = new();

    [ObservableProperty]
    private ObservableCollection<Produit> produits = new();

    [ObservableProperty]
    private string texteRecherche = string.Empty;

    [ObservableProperty]
    private Produit? produitSelectionne;

    [ObservableProperty]
    private bool afficherFormulaire;

    [ObservableProperty]
    private bool modeEdition;

    // Champs du formulaire d'ajout / édition
    [ObservableProperty] private string reference = string.Empty;
    [ObservableProperty] private string nom = string.Empty;
    [ObservableProperty] private string? categorie;
    [ObservableProperty] private string? description;
    [ObservableProperty] private decimal prixAchat;
    [ObservableProperty] private decimal prixVente;
    [ObservableProperty] private int quantiteStock;
    [ObservableProperty] private int seuilAlerte = 5;

    private int _idEnEdition;

    public string TitreFormulaire => ModeEdition ? "Modifier le produit" : "Nouveau produit";

    partial void OnModeEditionChanged(bool value) => OnPropertyChanged(nameof(TitreFormulaire));

    public StockViewModel()
    {
        DataChangeNotifier.DataChanged += OnDataChanged;
        _ = ChargerAsync();
    }

    private void OnDataChanged(object? sender, EventArgs e) => _ = ChargerAsync();

    [RelayCommand]
    private async Task ChargerAsync()
    {
        IsBusy = true;
        MessageErreur = null;
        try
        {
            _tousLesProduits = await _produitService.GetAllAsync();
            AppliquerFiltre();
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur de chargement : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnTexteRechercheChanged(string value) => AppliquerFiltre();

    private void AppliquerFiltre()
    {
        var filtre = _tousLesProduits.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(TexteRecherche))
        {
            var recherche = TexteRecherche.Trim();
            filtre = filtre.Where(p =>
                p.Nom.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                p.Reference.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                (p.Categorie?.Contains(recherche, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        Produits = new ObservableCollection<Produit>(filtre);
    }

    [RelayCommand]
    private void OuvrirNouveauFormulaire()
    {
        ModeEdition = false;
        _idEnEdition = 0;
        Reference = string.Empty;
        Nom = string.Empty;
        Categorie = null;
        Description = null;
        PrixAchat = 0;
        PrixVente = 0;
        QuantiteStock = 0;
        SeuilAlerte = 5;
        MessageErreur = null;
        AfficherFormulaire = true;
    }

    [RelayCommand]
    private void OuvrirEditionFormulaire(Produit? produit)
    {
        if (produit is null) return;

        ModeEdition = true;
        _idEnEdition = produit.Id;
        Reference = produit.Reference;
        Nom = produit.Nom;
        Categorie = produit.Categorie;
        Description = produit.Description;
        PrixAchat = produit.PrixAchat;
        PrixVente = produit.PrixVente;
        QuantiteStock = produit.QuantiteStock;
        SeuilAlerte = produit.SeuilAlerte;
        MessageErreur = null;
        AfficherFormulaire = true;
    }

    [RelayCommand]
    private void FermerFormulaire() => AfficherFormulaire = false;

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        if (string.IsNullOrWhiteSpace(Reference) || string.IsNullOrWhiteSpace(Nom))
        {
            MessageErreur = "La référence et le nom sont obligatoires.";
            return;
        }

        if (PrixVente < PrixAchat)
        {
            MessageErreur = "Le prix de vente ne peut pas être inférieur au prix d'achat.";
            return;
        }

        IsBusy = true;
        try
        {
            var doublon = await _produitService.ReferenceExisteAsync(Reference, ModeEdition ? _idEnEdition : null);
            if (doublon)
            {
                MessageErreur = "Cette référence existe déjà.";
                return;
            }

            if (ModeEdition)
            {
                await _produitService.UpdateAsync(new Produit
                {
                    Id = _idEnEdition,
                    Reference = Reference,
                    Nom = Nom,
                    Categorie = Categorie,
                    Description = Description,
                    PrixAchat = PrixAchat,
                    PrixVente = PrixVente,
                    QuantiteStock = QuantiteStock,
                    SeuilAlerte = SeuilAlerte
                });
            }
            else
            {
                await _produitService.AddAsync(new Produit
                {
                    Reference = Reference,
                    Nom = Nom,
                    Categorie = Categorie,
                    Description = Description,
                    PrixAchat = PrixAchat,
                    PrixVente = PrixVente,
                    QuantiteStock = QuantiteStock,
                    SeuilAlerte = SeuilAlerte
                });
            }

            AfficherFormulaire = false;
            await ChargerAsync();
            NotificationService.AfficherMessage(ModeEdition ? "✓ Produit modifié avec succès !" : "✓ Nouveau produit ajouté au stock !");
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur d'enregistrement : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SupprimerAsync(Produit? produit)
    {
        if (produit is null) return;

        var resultat = System.Windows.MessageBox.Show(
            $"Êtes-vous sûr de vouloir supprimer le produit « {produit.Nom} » (Réf: {produit.Reference}) ?\n\nCette action est irréversible.",
            "Confirmation de suppression - Kahuzi ERP",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (resultat != System.Windows.MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _produitService.DeleteAsync(produit.Id);
            await ChargerAsync();
            NotificationService.AfficherMessage("✓ Produit supprimé du stock.");
        }
        catch (Exception ex)
        {
            MessageErreur = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ExporterCsv()
    {
        try
        {
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Fichier CSV (*.csv)|*.csv",
                FileName = $"Inventaire_Stock_Kahuzi_{DateTime.Now:yyyyMMdd_HHmm}.csv",
                Title = "Exporter l'inventaire du stock"
            };

            if (sfd.ShowDialog() == true)
            {
                ExportService.ExporterProduitsCsv(Produits, sfd.FileName);
                NotificationService.AfficherMessage("✓ Inventaire du stock exporté en CSV avec succès !");
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur d'exportation : {ex.Message}";
        }
    }
}
