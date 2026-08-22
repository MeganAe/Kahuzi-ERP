using System.Reflection;

namespace GestionCommerciale.ViewModels;

public partial class AProposViewModel : ViewModelBase
{
    public string NomApplication => "Kahuzi ERP";

    public string Version =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

    public string Auteur => "Metoushela Walker";

    public string AnneeCourante => DateTime.Now.Year.ToString();
}