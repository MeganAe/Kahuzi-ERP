using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionCommerciale.Models;

public class Produit
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Reference { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Categorie { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrixAchat { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrixVente { get; set; }

    public int QuantiteStock { get; set; }

    public int SeuilAlerte { get; set; } = 5;

    public DateTime DateCreation { get; set; } = DateTime.Now;

    [NotMapped]
    public bool EnAlerte => QuantiteStock <= SeuilAlerte;

    [NotMapped]
    public decimal MargeUnitaire => PrixVente - PrixAchat;
}
