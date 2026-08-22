namespace GestionCommerciale.Data;

public static class DbInitializer
{
    public static void Initialize()
    {
        using var context = new AppDbContext();
        // Crée uniquement la structure : aucune donnée fictive n'est ajoutée.
        context.Database.EnsureCreated();
    }
}
