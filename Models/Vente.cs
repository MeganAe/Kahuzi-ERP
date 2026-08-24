using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionCommerciale.Models;

public enum StatutVente
{
    EnCours,
    Payee,
    Annulee
}

public class Vente
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string Numero { get; set; } = string.Empty;

    public int ClientId { get; set; }

    public Client? Client { get; set; }

    public DateTime DateVente { get; set; } = DateTime.Now;

    public StatutVente Statut { get; set; } = StatutVente.EnCours;

    [MaxLength(30)]
    public string ModePaiement { get; set; } = "Espèces";

    public List<LigneVente> Lignes { get; set; } = new();

    [NotMapped]
    public decimal Total => Lignes.Sum(l => l.SousTotal);

    [NotMapped]
    public bool EstAnnulable => Statut != StatutVente.Annulee;
}

public class LigneVente
{
    [Key]
    public int Id { get; set; }

    public int VenteId { get; set; }

    public Vente? Vente { get; set; }

    public int ProduitId { get; set; }

    public Produit? Produit { get; set; }

    public int Quantite { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrixUnitaire { get; set; }

    [NotMapped]
    public decimal SousTotal => Quantite * PrixUnitaire;

    [NotMapped]
    public string NomProduit => Produit?.Nom ?? $"Article #{ProduitId}";
}
