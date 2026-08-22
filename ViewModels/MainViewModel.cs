using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Services;

namespace GestionCommerciale.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ThemeService _themeService = new();

    [ObservableProperty]
    private ViewModelBase currentViewModel;

    [ObservableProperty]
    private string moduleActif = "Dashboard";

    [ObservableProperty]
    private bool isDarkTheme;

    private readonly DashboardViewModel _dashboardVm = new();
    private readonly StockViewModel _stockVm = new();
    private readonly ClientsViewModel _clientsVm = new();
    private readonly VentesViewModel _ventesVm = new();
    private readonly ParametresViewModel _parametresVm = new();
    private readonly AideViewModel _aideVm = new();
    private readonly AProposViewModel _aproposVm = new();

    public MainViewModel()
    {
        currentViewModel = _dashboardVm;
        _themeService.ApplyTheme(false);
    }

    [RelayCommand]
    private void NaviguerVers(string? module)
    {
        if (string.IsNullOrEmpty(module)) return;

        ModuleActif = module;

        CurrentViewModel = module switch
        {
            "Dashboard" => Rafraichir(_dashboardVm),
            "Stock" => Rafraichir(_stockVm),
            "Clients" => Rafraichir(_clientsVm),
            "Ventes" => Rafraichir(_ventesVm),
            "Parametres" => _parametresVm,
            "Aide" => _aideVm,
            "APropos" => _aproposVm,
            _ => CurrentViewModel
        };
    }

    private static ViewModelBase Rafraichir(ViewModelBase vm)
    {
        if (vm is DashboardViewModel d) _ = d.ChargerCommand.ExecuteAsync(null);
        return vm;
    }

    [RelayCommand]
    private void BasculerTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        _themeService.ApplyTheme(IsDarkTheme);
    }
}