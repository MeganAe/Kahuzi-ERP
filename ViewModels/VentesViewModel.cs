using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Models;
using GestionCommerciale.Services;

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

    [ObservableProperty]
    private ObservableCollection<Vente> ventes = new();

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
            Ventes = new ObservableCollection<Vente>(await _venteService.GetAllAsync());
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

            await _venteService.CreerVenteAsync(vente);
            AfficherFormulaire = false;
            await ChargerAsync();
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

        IsBusy = true;
        try
        {
            await _venteService.AnnulerVenteAsync(vente.Id);
            await ChargerAsync();
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
