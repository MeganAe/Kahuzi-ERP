using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Models;
using GestionCommerciale.Services;

namespace GestionCommerciale.ViewModels;

public class TopProduitItem
{
    public string Nom { get; set; } = string.Empty;
    public int QuantiteVendue { get; set; }
    public decimal TotalVente { get; set; }
}

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ProduitService _produitService = new();
    private readonly ClientService _clientService = new();
    private readonly VenteService _venteService = new();

    private List<Produit> _tousLesProduits = new();
    private List<Vente> _toutesLesVentes = new();

    [ObservableProperty] private int nombreProduits;
    [ObservableProperty] private int nombreClients;
    [ObservableProperty] private int nombreVentes;
    [ObservableProperty] private decimal chiffreAffairesTotal;
    [ObservableProperty] private decimal beneficeNetTotal;
    [ObservableProperty] private decimal valeurStockAchat;
    [ObservableProperty] private decimal valeurStockVente;
    [ObservableProperty] private int produitsEnAlerte;

    [ObservableProperty] private List<Produit> produitsAlerteListe = new();
    [ObservableProperty] private List<Vente> dernieresVentes = new();
    [ObservableProperty] private List<TopProduitItem> topProduitsVendus = new();

    public List<string> PeriodesFiltre { get; } = new() { "Tout l'historique", "Aujourd'hui", "7 derniers jours", "30 derniers jours" };

    [ObservableProperty]
    private string periodeChoisie = "Tout l'historique";

    partial void OnPeriodeChoisieChanged(string value) => CalculerStatistiques();

    public DashboardViewModel()
    {
        DataChangeNotifier.DataChanged += OnDataChanged;
        _ = ChargerAsync();
    }

    private void OnDataChanged(object? sender, EventArgs e) => _ = ChargerAsync();

    [RelayCommand]
    private async Task ChargerAsync()
    {
        IsBusy = true;
        try
        {
            _tousLesProduits = await _produitService.GetAllAsync();
            var clients = await _clientService.GetAllAsync();
            _toutesLesVentes = await _venteService.GetAllAsync();

            NombreProduits = _tousLesProduits.Count;
            NombreClients = clients.Count;

            // Valeur du stock actuel
            ValeurStockAchat = _tousLesProduits.Sum(p => p.PrixAchat * p.QuantiteStock);
            ValeurStockVente = _tousLesProduits.Sum(p => p.PrixVente * p.QuantiteStock);

            ProduitsAlerteListe = _tousLesProduits.Where(p => p.EnAlerte).ToList();
            ProduitsEnAlerte = ProduitsAlerteListe.Count;

            DernieresVentes = _toutesLesVentes.Take(6).ToList();

            CalculerStatistiques();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CalculerStatistiques()
    {
        var ventesFiltrees = _toutesLesVentes.Where(v => v.Statut != StatutVente.Annulee);
        var now = DateTime.Now;

        ventesFiltrees = PeriodeChoisie switch
        {
            "Aujourd'hui" => ventesFiltrees.Where(v => v.DateVente.Date == now.Date),
            "7 derniers jours" => ventesFiltrees.Where(v => v.DateVente >= now.AddDays(-7)),
            "30 derniers jours" => ventesFiltrees.Where(v => v.DateVente >= now.AddDays(-30)),
            _ => ventesFiltrees
        };

        var listeVentes = ventesFiltrees.ToList();
        NombreVentes = listeVentes.Count;
        ChiffreAffairesTotal = listeVentes.Where(v => v.Statut == StatutVente.Payee).Sum(v => v.Total);

        // Dictionnaire des prix d'achat
        var dictionnairePrixAchat = _tousLesProduits.ToDictionary(p => p.Id, p => p.PrixAchat);

        // Calcul du coût d'achat des produits vendus et bénéfice net
        decimal coutTotalAchat = 0;
        var statsArticles = new Dictionary<string, (int Qte, decimal Total)>();

        foreach (var v in listeVentes.Where(v => v.Statut == StatutVente.Payee))
        {
            if (v.Lignes != null)
            {
                foreach (var l in v.Lignes)
                {
                    var prixAchatUnitaire = dictionnairePrixAchat.TryGetValue(l.ProduitId, out var pa) ? pa : (l.Produit?.PrixAchat ?? 0);
                    coutTotalAchat += prixAchatUnitaire * l.Quantite;

                    if (!statsArticles.ContainsKey(l.NomProduit))
                    {
                        statsArticles[l.NomProduit] = (0, 0);
                    }
                    var current = statsArticles[l.NomProduit];
                    statsArticles[l.NomProduit] = (current.Qte + l.Quantite, current.Total + l.SousTotal);
                }
            }
        }

        BeneficeNetTotal = Math.Max(0, ChiffreAffairesTotal - coutTotalAchat);

        TopProduitsVendus = statsArticles
            .OrderByDescending(kv => kv.Value.Qte)
            .Take(5)
            .Select(kv => new TopProduitItem
            {
                Nom = kv.Key,
                QuantiteVendue = kv.Value.Qte,
                TotalVente = kv.Value.Total
            })
            .ToList();
    }
}
