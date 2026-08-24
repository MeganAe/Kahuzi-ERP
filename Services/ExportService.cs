using System.IO;
using System.Text;
using GestionCommerciale.Data;
using GestionCommerciale.Models;

namespace GestionCommerciale.Services;

public class ExportService
{
    public static void ExporterProduitsCsv(IEnumerable<Produit> produits, string cheminFichier)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID;Reference;Nom;Categorie;PrixAchat;PrixVente;Stock;SeuilAlerte;Description");

        foreach (var p in produits)
        {
            sb.AppendLine($"{p.Id};\"{Echapper(p.Reference)}\";\"{Echapper(p.Nom)}\";\"{Echapper(p.Categorie)}\";{p.PrixAchat};{p.PrixVente};{p.QuantiteStock};{p.SeuilAlerte};\"{Echapper(p.Description)}\"");
        }

        File.WriteAllText(cheminFichier, sb.ToString(), Encoding.UTF8);
    }

    public static void ExporterClientsCsv(IEnumerable<Client> clients, string cheminFichier)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID;Nom;Prenom;Telephone;Email;Adresse");

        foreach (var c in clients)
        {
            sb.AppendLine($"{c.Id};\"{Echapper(c.Nom)}\";\"{Echapper(c.Prenom)}\";\"{Echapper(c.Telephone)}\";\"{Echapper(c.Email)}\";\"{Echapper(c.Adresse)}\"");
        }

        File.WriteAllText(cheminFichier, sb.ToString(), Encoding.UTF8);
    }

    public static void ExporterVentesCsv(IEnumerable<Vente> ventes, string cheminFichier)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID;Numero;Date;Client;ModePaiement;Statut;Total;NombreArticles");

        foreach (var v in ventes)
        {
            var nomClient = v.Client?.NomComplet ?? "Client occasionnel";
            var nbArticles = v.Lignes?.Sum(l => l.Quantite) ?? 0;
            sb.AppendLine($"{v.Id};\"{Echapper(v.Numero)}\";{v.DateVente:yyyy-MM-dd HH:mm:ss};\"{Echapper(nomClient)}\";{v.ModePaiement};{v.Statut};{v.Total};{nbArticles}");
        }

        File.WriteAllText(cheminFichier, sb.ToString(), Encoding.UTF8);
    }

    public static void SauvegarderBase(string cheminDestination)
    {
        var sourcePath = AppDbContext.DbPath;
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, cheminDestination, overwrite: true);
        }
        else
        {
            throw new FileNotFoundException("Le fichier de base de données est introuvable.");
        }
    }

    public static void RestaurerBase(string cheminSource)
    {
        var destPath = AppDbContext.DbPath;
        if (File.Exists(cheminSource))
        {
            var dir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.Copy(cheminSource, destPath, overwrite: true);
            DataChangeNotifier.Notify();
        }
        else
        {
            throw new FileNotFoundException("Le fichier source de restauration est introuvable.");
        }
    }

    private static string Echapper(string? texte)
    {
        if (string.IsNullOrEmpty(texte)) return string.Empty;
        return texte.Replace("\"", "\"\"");
    }
}
