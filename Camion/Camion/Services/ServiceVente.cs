using Camion.Data;
using Microsoft.EntityFrameworkCore;

namespace Camion.Services;

/// <summary>
/// Applique les règles métier de vente et d'échange des voitures.
/// </summary>
public class ServiceVente
{
    private readonly GarageDbContext _contexte;

    public ServiceVente(GarageDbContext contexte)
    {
        _contexte = contexte;
    }

    /// <summary>
    /// Vend une voiture disponible à un client si elle respecte la limite de deux voitures.
    /// </summary>
    public async Task<ResultatOperation> VendreAsync(
        Guid idVoiture,
        string emailClient,
        DateTime dateVente,
        CancellationToken cancellationToken = default)
    {
        var voiture = await _contexte.Voitures
            .Include(element => element.Clients)
            .SingleOrDefaultAsync(element => element.Id == idVoiture, cancellationToken);
        var client = await _contexte.Clients
            .Include(element => element.Voitures)
            .SingleOrDefaultAsync(element => element.Email == emailClient, cancellationToken);

        if (voiture is null)
        {
            return new ResultatOperation(false, "Voiture introuvable.");
        }

        if (client is null)
        {
            return new ResultatOperation(false, "Client introuvable.");
        }

        if (client.Voitures.Count >= 2)
        {
            return new ResultatOperation(false, "Un client ne peut pas posséder plus de deux voitures.");
        }

        if (voiture.EstVendue)
        {
            return new ResultatOperation(false, "La voiture est déjà vendue.");
        }

        voiture.Clients.Add(client);
        voiture.EstVendue = true;
        voiture.DateVente = dateVente;
        await _contexte.SaveChangesAsync(cancellationToken);

        return new ResultatOperation(true, "La voiture a été vendue.");
    }

    /// <summary>
    /// Échange une voiture vendue appartenant à un client contre une voiture disponible.
    /// </summary>
    public async Task<ResultatOperation> EchangerAsync(
        Guid idAncienneVoiture,
        Guid idNouvelleVoiture,
        string emailClient,
        DateTime dateEchange,
        CancellationToken cancellationToken = default)
    {
        if (idAncienneVoiture == idNouvelleVoiture)
        {
            return new ResultatOperation(false, "Les deux voitures doivent être différentes.");
        }

        var client = await _contexte.Clients
            .Include(element => element.Voitures)
            .SingleOrDefaultAsync(element => element.Email == emailClient, cancellationToken);
        var ancienneVoiture = await _contexte.Voitures
            .Include(element => element.Clients)
            .SingleOrDefaultAsync(element => element.Id == idAncienneVoiture, cancellationToken);
        var nouvelleVoiture = await _contexte.Voitures
            .Include(element => element.Clients)
            .SingleOrDefaultAsync(element => element.Id == idNouvelleVoiture, cancellationToken);

        if (client is null)
        {
            return new ResultatOperation(false, "Client introuvable.");
        }

        if (ancienneVoiture is null || nouvelleVoiture is null)
        {
            return new ResultatOperation(false, "Voiture introuvable.");
        }

        if (!ancienneVoiture.EstVendue || !ancienneVoiture.Clients.Any(element => element.Id == client.Id))
        {
            return new ResultatOperation(false, "Cette voiture vendue n'appartient pas au client.");
        }

        if (nouvelleVoiture.EstVendue)
        {
            return new ResultatOperation(false, "La nouvelle voiture est déjà vendue.");
        }

        ancienneVoiture.Clients.Remove(client);
        if (ancienneVoiture.Clients.Count == 0)
        {
            ancienneVoiture.EstVendue = false;
            ancienneVoiture.DateVente = null;
        }

        nouvelleVoiture.Clients.Add(client);
        nouvelleVoiture.EstVendue = true;
        nouvelleVoiture.DateVente = dateEchange;
        await _contexte.SaveChangesAsync(cancellationToken);

        return new ResultatOperation(true, "L'échange a été enregistré.");
    }
}

public record ResultatOperation(bool Succes, string Message);
