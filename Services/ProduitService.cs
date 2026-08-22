using Microsoft.EntityFrameworkCore;
using GestionCommerciale.Data;
using GestionCommerciale.Models;

namespace GestionCommerciale.Services;

public class ProduitService
{
    public async Task<List<Produit>> GetAllAsync()
    {
        using var ctx = new AppDbContext();
        return await ctx.Produits.OrderBy(p => p.Nom).ToListAsync();
    }

    public async Task<Produit> AddAsync(Produit produit)
    {
        using var ctx = new AppDbContext();
        ctx.Produits.Add(produit);
        await ctx.SaveChangesAsync();
        DataChangeNotifier.Notify();
        return produit;
    }

    public async Task UpdateAsync(Produit produit)
    {
        using var ctx = new AppDbContext();
        ctx.Produits.Update(produit);
        await ctx.SaveChangesAsync();
        DataChangeNotifier.Notify();
    }

    public async Task DeleteAsync(int id)
    {
        using var ctx = new AppDbContext();
        var produit = await ctx.Produits.FindAsync(id);
        if (produit is null) return;

        var lignes = await ctx.LignesVente.Where(l => l.ProduitId == id).ToListAsync();
        if (lignes.Count > 0)
        {
            ctx.LignesVente.RemoveRange(lignes);
        }

        ctx.Produits.Remove(produit);
        await ctx.SaveChangesAsync();
        DataChangeNotifier.Notify();
    }

    public async Task<bool> ReferenceExisteAsync(string reference, int? excludeId = null)
    {
        using var ctx = new AppDbContext();
        return await ctx.Produits.AnyAsync(p => p.Reference == reference && p.Id != excludeId);
    }
}
