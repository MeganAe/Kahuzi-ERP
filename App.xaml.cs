using System.Windows;
using GestionCommerciale.Data;

namespace GestionCommerciale;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Crée la base SQLite et les tables si elles n'existent pas encore.
        DbInitializer.Initialize();
    }
}
