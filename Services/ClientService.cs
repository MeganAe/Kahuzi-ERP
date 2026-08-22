using Microsoft.EntityFrameworkCore;
using GestionCommerciale.Data;
using GestionCommerciale.Models;

namespace GestionCommerciale.Services;

public class ClientService
{
    public async Task<List<Client>> GetAllAsync()
    {
        using var ctx = new AppDbContext();
        return await ctx.Clients.OrderBy(c => c.Nom).ToListAsync();
    }

    public async Task<Client> AddAsync(Client client)
    {
        using var ctx = new AppDbContext();
        ctx.Clients.Add(client);
        await ctx.SaveChangesAsync();
        DataChangeNotifier.Notify();
        return client;
    }

    public async Task UpdateAsync(Client client)
    {
        using var ctx = new AppDbContext();
        ctx.Clients.Update(client);
        await ctx.SaveChangesAsync();
        DataChangeNotifier.Notify();
    }

    public async Task DeleteAsync(int id)
    {
        using var ctx = new AppDbContext();
        var client = await ctx.Clients
            .Include(c => c.Ventes)
            .ThenInclude(v => v.Lignes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client is null) return;

        if (client.Ventes.Count > 0)
        {
            foreach (var v in client.Ventes)
            {
                ctx.LignesVente.RemoveRange(v.Lignes);
            }
            ctx.Ventes.RemoveRange(client.Ventes);
        }

        ctx.Clients.Remove(client);
        await ctx.SaveChangesAsync();
        DataChangeNotifier.Notify();
    }
}
