using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Models;
using GestionCommerciale.Services;

namespace GestionCommerciale.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    private readonly ClientService _clientService = new();
    private List<Client> _tousLesClients = new();

    [ObservableProperty]
    private ObservableCollection<Client> clients = new();

    [ObservableProperty]
    private string texteRecherche = string.Empty;

    [ObservableProperty]
    private bool afficherFormulaire;

    [ObservableProperty]
    private bool modeEdition;

    [ObservableProperty] private string nom = string.Empty;
    [ObservableProperty] private string? prenom;
    [ObservableProperty] private string? telephone;
    [ObservableProperty] private string? email;
    [ObservableProperty] private string? adresse;

    private int _idEnEdition;

    public string TitreFormulaire => ModeEdition ? "Modifier le client" : "Nouveau client";

    partial void OnModeEditionChanged(bool value) => OnPropertyChanged(nameof(TitreFormulaire));

    public ClientsViewModel()
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
            _tousLesClients = await _clientService.GetAllAsync();
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
        var filtre = _tousLesClients.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(TexteRecherche))
        {
            var recherche = TexteRecherche.Trim();
            filtre = filtre.Where(c =>
                c.NomComplet.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                (c.Telephone?.Contains(recherche, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Email?.Contains(recherche, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        Clients = new ObservableCollection<Client>(filtre);
    }

    [RelayCommand]
    private void OuvrirNouveauFormulaire()
    {
        ModeEdition = false;
        _idEnEdition = 0;
        Nom = string.Empty;
        Prenom = null;
        Telephone = null;
        Email = null;
        Adresse = null;
        MessageErreur = null;
        AfficherFormulaire = true;
    }

    [RelayCommand]
    private void OuvrirEditionFormulaire(Client? client)
    {
        if (client is null) return;

        ModeEdition = true;
        _idEnEdition = client.Id;
        Nom = client.Nom;
        Prenom = client.Prenom;
        Telephone = client.Telephone;
        Email = client.Email;
        Adresse = client.Adresse;
        MessageErreur = null;
        AfficherFormulaire = true;
    }

    [RelayCommand]
    private void FermerFormulaire() => AfficherFormulaire = false;

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        if (string.IsNullOrWhiteSpace(Nom))
        {
            MessageErreur = "Le nom est obligatoire.";
            return;
        }

        IsBusy = true;
        try
        {
            if (ModeEdition)
            {
                await _clientService.UpdateAsync(new Client
                {
                    Id = _idEnEdition,
                    Nom = Nom,
                    Prenom = Prenom,
                    Telephone = Telephone,
                    Email = Email,
                    Adresse = Adresse
                });
            }
            else
            {
                await _clientService.AddAsync(new Client
                {
                    Nom = Nom,
                    Prenom = Prenom,
                    Telephone = Telephone,
                    Email = Email,
                    Adresse = Adresse
                });
            }

            AfficherFormulaire = false;
            await ChargerAsync();
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
    private async Task SupprimerAsync(Client? client)
    {
        if (client is null) return;

        var resultat = System.Windows.MessageBox.Show(
            $"Êtes-vous sûr de vouloir supprimer le client « {client.NomComplet} » ?\n\nSes éventuelles ventes associées seront également supprimées.",
            "Confirmation de suppression - Kahuzi ERP",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (resultat != System.Windows.MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            await _clientService.DeleteAsync(client.Id);
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
