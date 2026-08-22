namespace GestionCommerciale.Services;

/// <summary>Notifie les écrans lorsqu'une donnée SQLite est modifiée.</summary>
public static class DataChangeNotifier
{
    public static event EventHandler? DataChanged;

    public static void Notify() => DataChanged?.Invoke(null, EventArgs.Empty);
}
