using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Models;
using GestionCommerciale.Services;

namespace GestionCommerciale.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ProduitService _produitService = new();
    private readonly ClientService _clientService = new();
    private readonly VenteService _venteService = new();

    [ObservableProperty] private int nombreProduits;
    [ObservableProperty] private int nombreClients;
    [ObservableProperty] private int nombreVentes;
    [ObservableProperty] private decimal chiffreAffairesTotal;
    [ObservableProperty] private int produitsEnAlerte;
    [ObservableProperty] private List<Produit> produitsAlerteListe = new();
    [ObservableProperty] private List<Vente> dernieresVentes = new();

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
            var produits = await _produitService.GetAllAsync();
            var clients = await _clientService.GetAllAsync();
            var ventes = await _venteService.GetAllAsync();

            NombreProduits = produits.Count;
            NombreClients = clients.Count;
            NombreVentes = ventes.Count(v => v.Statut != StatutVente.Annulee);
            ChiffreAffairesTotal = ventes.Where(v => v.Statut == StatutVente.Payee).Sum(v => v.Total);

            ProduitsAlerteListe = produits.Where(p => p.EnAlerte).ToList();
            ProduitsEnAlerte = ProduitsAlerteListe.Count;

            DernieresVentes = ventes.Take(5).ToList();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
