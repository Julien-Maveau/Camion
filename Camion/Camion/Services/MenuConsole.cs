using System.Globalization;
using Camion.Data;
using Microsoft.EntityFrameworkCore;

namespace Camion.Services;

/// <summary>
/// Gère l'affichage et les choix du menu de l'application console.
/// </summary>
public class MenuConsole
{
    private readonly GarageDbContext _contexte;
    private readonly TextReader _entree;
    private readonly TextWriter _sortie;

    public MenuConsole(GarageDbContext contexte, TextReader entree, TextWriter sortie)
    {
        _contexte = contexte;
        _entree = entree;
        _sortie = sortie;
    }

    public async Task ExecuterAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            AfficherChoix();
            var choix = _entree.ReadLine();

            switch (choix)
            {
                case "1":
                    await AfficherClientsAsync(cancellationToken);
                    break;
                case "2":
                    await AfficherVoituresAsync(cancellationToken);
                    break;
                case "0":
                    _sortie.WriteLine("Au revoir.");
                    return;
                default:
                    _sortie.WriteLine("Choix invalide.");
                    break;
            }
        }
    }

    private void AfficherChoix()
    {
        _sortie.WriteLine();
        _sortie.WriteLine("=== Garage Camion ===");
        _sortie.WriteLine("1. Afficher les clients");
        _sortie.WriteLine("2. Afficher les voitures");
        _sortie.WriteLine("0. Quitter");
        _sortie.Write("Votre choix : ");
    }

    private async Task AfficherClientsAsync(CancellationToken cancellationToken)
    {
        var clients = await _contexte.Clients
            .Include(client => client.Voitures)
            .OrderBy(client => client.Nom)
            .ThenBy(client => client.Prenom)
            .ToListAsync(cancellationToken);

        _sortie.WriteLine();
        _sortie.WriteLine("--- Liste des clients ---");
        foreach (var client in clients)
        {
            _sortie.WriteLine(
                $"{client.Nom} {client.Prenom} | {client.Email} | {client.Voitures.Count} voiture(s)");
        }
    }

    private async Task AfficherVoituresAsync(CancellationToken cancellationToken)
    {
        var voitures = await _contexte.Voitures
            .OrderBy(voiture => voiture.Marque)
            .ThenBy(voiture => voiture.Modele)
            .ToListAsync(cancellationToken);

        _sortie.WriteLine();
        _sortie.WriteLine("--- Liste des voitures ---");
        foreach (var voiture in voitures)
        {
            var etat = voiture.EstVendue ? "Vendue" : "Disponible";
            _sortie.WriteLine(
                $"{voiture.Marque} {voiture.Modele} ({voiture.Annee}) | {voiture.PrixHt.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"))} € HT | {etat}");
        }
    }
}
