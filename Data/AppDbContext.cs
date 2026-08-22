using System.IO;
using Microsoft.EntityFrameworkCore;
using GestionCommerciale.Models;

namespace GestionCommerciale.Data;

public class AppDbContext : DbContext
{
    // Base placée dans le dossier de l'utilisateur pour éviter les soucis
    // de droits d'écriture dans le dossier d'installation.
    public static string DbPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KahuziERP", "kahuzi_erp.db");

    public DbSet<Produit> Produits => Set<Produit>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Vente> Ventes => Set<Vente>();
    public DbSet<LigneVente> LignesVente => Set<LigneVente>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vente>()
            .HasOne(v => v.Client)
            .WithMany(c => c.Ventes)
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LigneVente>()
            .HasOne(l => l.Vente)
            .WithMany(v => v.Lignes)
            .HasForeignKey(l => l.VenteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LigneVente>()
            .HasOne(l => l.Produit)
            .WithMany()
            .HasForeignKey(l => l.ProduitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Produit>()
            .HasIndex(p => p.Reference)
            .IsUnique();
    }
}