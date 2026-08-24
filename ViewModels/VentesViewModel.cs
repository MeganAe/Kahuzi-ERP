using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Models;
using GestionCommerciale.Services;
using Microsoft.Win32;

namespace GestionCommerciale.ViewModels;

public partial class LignePanier : ObservableObject
{
    public int ProduitId { get; init; }
    public string NomProduit { get; init; } = string.Empty;
    public decimal PrixUnitaire { get; init; }
    public int StockDisponible { get; init; }

    [ObservableProperty]
    private int quantite = 1;

    public decimal SousTotal => Quantite * PrixUnitaire;
}

public partial class VentesViewModel : ViewModelBase
{
    private readonly VenteService _venteService = new();
    private readonly ProduitService _produitService = new();
    private readonly ClientService _clientService = new();

    private List<Vente> _toutesLesVentes = new();

    [ObservableProperty]
    private ObservableCollection<Vente> ventes = new();

    [ObservableProperty]
    private string texteRecherche = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Produit> produitsDisponibles = new();

    [ObservableProperty]
    private ObservableCollection<Client> clientsDisponibles = new();

    [ObservableProperty]
    private ObservableCollection<LignePanier> panier = new();

    [ObservableProperty]
    private Produit? produitChoisi;

    [ObservableProperty]
    private int quantiteChoisie = 1;

    [ObservableProperty]
    private Client? clientChoisi;

    [ObservableProperty]
    private string modePaiement = "Espèces";

    [ObservableProperty]
    private bool afficherFormulaire;

    [ObservableProperty]
    private bool afficherDetailsVente;

    [ObservableProperty]
    private Vente? venteDetails;

    public decimal TotalPanier => Panier.Sum(l => l.SousTotal);

    public List<string> ModePaiementListe { get; } = new() { "Espèces", "Mobile Money", "Virement", "Carte" };

    public VentesViewModel()
    {
        DataChangeNotifier.DataChanged += OnDataChanged;
        _ = ChargerAsync();
        Panier.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalPanier));
    }

    private void OnDataChanged(object? sender, EventArgs e) => _ = ChargerAsync();

    [RelayCommand]
    private async Task ChargerAsync()
    {
        IsBusy = true;
        MessageErreur = null;
        try
        {
            _toutesLesVentes = await _venteService.GetAllAsync();
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
        var filtre = _toutesLesVentes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(TexteRecherche))
        {
            var recherche = TexteRecherche.Trim();
            filtre = filtre.Where(v =>
                v.Numero.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                (v.Client?.NomComplet.Contains(recherche, StringComparison.OrdinalIgnoreCase) ?? false) ||
                v.ModePaiement.Contains(recherche, StringComparison.OrdinalIgnoreCase));
        }

        Ventes = new ObservableCollection<Vente>(filtre);
    }

    [RelayCommand]
    private async Task OuvrirNouvelleVenteAsync()
    {
        MessageErreur = null;
        Panier.Clear();
        QuantiteChoisie = 1;
        ClientChoisi = null;
        ProduitChoisi = null;
        ModePaiement = "Espèces";

        ProduitsDisponibles = new ObservableCollection<Produit>(
            (await _produitService.GetAllAsync()).Where(p => p.QuantiteStock > 0));
        ClientsDisponibles = new ObservableCollection<Client>(await _clientService.GetAllAsync());

        AfficherFormulaire = true;
    }

    [RelayCommand]
    private void FermerFormulaire() => AfficherFormulaire = false;

    [RelayCommand]
    private void VoirDetailsVente(Vente? vente)
    {
        if (vente is null) return;
        VenteDetails = vente;
        AfficherDetailsVente = true;
    }

    [RelayCommand]
    private void FermerDetailsVente() => AfficherDetailsVente = false;

    [RelayCommand]
    private void ImprimerFacture(Vente? vente)
    {
        var v = vente ?? VenteDetails;
        if (v is null) return;

        try
        {
            ImpressionService.ImprimerFacture(v);
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur d'impression : {ex.Message}";
        }
    }

    [RelayCommand]
    private void ExporterCsv()
    {
        try
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Fichier CSV (*.csv)|*.csv",
                FileName = $"Journal_Ventes_Kahuzi_{DateTime.Now:yyyyMMdd_HHmm}.csv",
                Title = "Exporter le journal des ventes"
            };

            if (sfd.ShowDialog() == true)
            {
                ExportService.ExporterVentesCsv(Ventes, sfd.FileName);
                NotificationService.AfficherMessage("✓ Journal des ventes exporté en CSV avec succès !");
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur d'exportation : {ex.Message}";
        }
    }

    [RelayCommand]
    private void AjouterAuPanier()
    {
        if (ProduitChoisi is null || QuantiteChoisie <= 0)
        {
            MessageErreur = "Choisis un produit et une quantité valide.";
            return;
        }

        var ligneExistante = Panier.FirstOrDefault(l => l.ProduitId == ProduitChoisi.Id);
        var quantiteDejaDansPanier = ligneExistante?.Quantite ?? 0;

        if (quantiteDejaDansPanier + QuantiteChoisie > ProduitChoisi.QuantiteStock)
        {
            MessageErreur = $"Stock insuffisant pour « {ProduitChoisi.Nom} » (disponible : {ProduitChoisi.QuantiteStock}).";
            return;
        }

        MessageErreur = null;

        if (ligneExistante is not null)
        {
            ligneExistante.Quantite += QuantiteChoisie;
        }
        else
        {
            Panier.Add(new LignePanier
            {
                ProduitId = ProduitChoisi.Id,
                NomProduit = ProduitChoisi.Nom,
                PrixUnitaire = ProduitChoisi.PrixVente,
                StockDisponible = ProduitChoisi.QuantiteStock,
                Quantite = QuantiteChoisie
            });
        }

        OnPropertyChanged(nameof(TotalPanier));
        QuantiteChoisie = 1;
    }

    [RelayCommand]
    private void RetirerDuPanier(LignePanier? ligne)
    {
        if (ligne is null) return;
        Panier.Remove(ligne);
        OnPropertyChanged(nameof(TotalPanier));
    }

    [RelayCommand]
    private async Task ValiderVenteAsync()
    {
        if (ClientChoisi is null)
        {
            MessageErreur = "Sélectionne un client.";
            return;
        }

        if (Panier.Count == 0)
        {
            MessageErreur = "Le panier est vide.";
            return;
        }

        IsBusy = true;
        try
        {
            var vente = new Vente
            {
                Numero = await _venteService.GenererNumeroAsync(),
                ClientId = ClientChoisi.Id,
                DateVente = DateTime.Now,
                Statut = StatutVente.Payee,
                ModePaiement = ModePaiement,
                Lignes = Panier.Select(l => new LigneVente
                {
                    ProduitId = l.ProduitId,
                    Quantite = l.Quantite,
                    PrixUnitaire = l.PrixUnitaire
                }).ToList()
            };

            var venteCreee = await _venteService.CreerVenteAsync(vente);
            AfficherFormulaire = false;
            await ChargerAsync();
            NotificationService.AfficherMessage($"✓ Vente {venteCreee.Numero} enregistrée ({venteCreee.Total:N0} FC) !");

            // Proposer d'imprimer la facture immédiatement
            var rep = System.Windows.MessageBox.Show(
                $"Vente {venteCreee.Numero} enregistrée avec succès !\n\nSouhaitez-vous imprimer la facture / reçu maintenant ?",
                "Impression Facture - Kahuzi ERP",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (rep == System.Windows.MessageBoxResult.Yes)
            {
                // Recharger avec client et lignes pour l'impression
                var venteComplete = _toutesLesVentes.FirstOrDefault(v => v.Id == venteCreee.Id) ?? venteCreee;
                ImpressionService.ImprimerFacture(venteComplete);
            }
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
    private async Task AnnulerVenteAsync(Vente? vente)
    {
        if (vente is null) return;

        var rep = System.Windows.MessageBox.Show(
            $"Êtes-vous sûr de vouloir annuler la vente {vente.Numero} ?\nLes articles seront réintégrés en stock.",
            "Confirmation d'annulation",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (rep != System.Windows.MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _venteService.AnnulerVenteAsync(vente.Id);
            await ChargerAsync();
            NotificationService.AfficherMessage($"✓ Vente {vente.Numero} annulée.");
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
}
