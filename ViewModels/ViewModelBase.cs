using CommunityToolkit.Mvvm.ComponentModel;

namespace GestionCommerciale.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? messageErreur;
}
