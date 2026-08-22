using Microsoft.EntityFrameworkCore;
using GestionCommerciale.Data;
using GestionCommerciale.Models;

namespace GestionCommerciale.Services;

public class VenteService
{
    public async Task<List<Vente>> GetAllAsync()
    {
        using var ctx = new AppDbContext();
        return await ctx.Ventes
            .Include(v => v.Client)
            .Include(v => v.Lignes)
                .ThenInclude(l => l.Produit)
            .OrderByDescending(v => v.DateVente)
            .ToListAsync();
    }

    public async Task<string> GenererNumeroAsync()
    {
        using var ctx = new AppDbContext();
        var count = await ctx.Ventes.CountAsync();
        return $"V-{DateTime.Now:yyyyMMdd}-{count + 1:0000}";
    }

    /// <summary>
    /// Enregistre une vente et décrémente le stock des produits concernés.
    /// Lève une exception si le stock est insuffisant pour l'une des lignes.
    /// </summary>
    public async Task<Vente> CreerVenteAsync(Vente vente)
    {
        using var ctx = new AppDbContext();
        using var transaction = await ctx.Database.BeginTransactionAsync();

        try
        {
            foreach (var ligne in vente.Lignes)
            {
                var produit = await ctx.Produits.FindAsync(ligne.ProduitId)
                    ?? throw new InvalidOperationException("Produit introuvable.");

                if (produit.QuantiteStock < ligne.Quantite)
                    throw new InvalidOperationException($"Stock insuffisant pour « {produit.Nom} » (disponible : {produit.QuantiteStock}).");

                produit.QuantiteStock -= ligne.Quantite;
                ligne.PrixUnitaire = produit.PrixVente;
                ligne.Produit = null;
            }

            vente.Client = null;
            ctx.Ventes.Add(vente);

            await ctx.SaveChangesAsync();
            await transaction.CommitAsync();
            DataChangeNotifier.Notify();
            return vente;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task AnnulerVenteAsync(int venteId)
    {
        using var ctx = new AppDbContext();
        using var transaction = await ctx.Database.BeginTransactionAsync();

        var vente = await ctx.Ventes.Include(v => v.Lignes).FirstOrDefaultAsync(v => v.Id == venteId);
        if (vente is null || vente.Statut == StatutVente.Annulee) return;

        foreach (var ligne in vente.Lignes)
        {
            var produit = await ctx.Produits.FindAsync(ligne.ProduitId);
            if (produit is not null)
                produit.QuantiteStock += ligne.Quantite;
        }

        vente.Statut = StatutVente.Annulee;
        await ctx.SaveChangesAsync();
        await transaction.CommitAsync();
        DataChangeNotifier.Notify();
    }
}
