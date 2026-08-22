using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionCommerciale.Models;

public class Client
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Prenom { get; set; }

    [MaxLength(30)]
    public string? Telephone { get; set; }

    [MaxLength(120)]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Adresse { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.Now;

    public List<Vente> Ventes { get; set; } = new();

    [NotMapped]
    public string NomComplet => string.IsNullOrWhiteSpace(Prenom) ? Nom : $"{Nom} {Prenom}";
}
