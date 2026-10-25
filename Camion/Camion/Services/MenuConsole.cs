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
            if (choix is null)
            {
                _sortie.WriteLine("Au revoir.");
                return;
            }

            switch (choix)
            {
                case "1":
                    await AfficherClientsAsync(cancellationToken);
                    break;
                case "2":
                    await AfficherVoituresAsync(cancellationToken);
                    break;
                case "3":
                    await AfficherFicheClientAsync(cancellationToken);
                    break;
                case "4":
                    await AfficherFicheVoitureAsync(cancellationToken);
                    break;
                case "5":
                    await VendreVoitureAsync(cancellationToken);
                    break;
                case "6":
                    await EchangerVoitureAsync(cancellationToken);
                    break;
                case "7":
                    await AfficherChiffreAffairesHebdomadaireAsync(cancellationToken);
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
        _sortie.WriteLine("3. Consulter la fiche d'un client");
        _sortie.WriteLine("4. Consulter la fiche d'une voiture");
        _sortie.WriteLine("5. Vendre une voiture");
        _sortie.WriteLine("6. Échanger une voiture");
        _sortie.WriteLine("7. Afficher le chiffre d'affaires hebdomadaire");
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
                $"[{voiture.Id}] {voiture.Marque} {voiture.Modele} ({voiture.Annee}) | {voiture.PrixHt.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"))} € HT | {etat}");
        }
    }

    private async Task AfficherFicheClientAsync(CancellationToken cancellationToken)
    {
        _sortie.Write("E-mail du client : ");
        var email = _entree.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            _sortie.WriteLine("L'e-mail est obligatoire.");
            return;
        }

        var client = await _contexte.Clients
            .Include(element => element.Voitures)
            .SingleOrDefaultAsync(element => element.Email == email.Trim(), cancellationToken);

        if (client is null)
        {
            _sortie.WriteLine("Client introuvable.");
            return;
        }

        _sortie.WriteLine();
        _sortie.WriteLine("--- Fiche client ---");
        _sortie.WriteLine($"Nom : {client.Nom} {client.Prenom}");
        _sortie.WriteLine($"Date de naissance : {client.DateNaissance:dd/MM/yyyy}");
        _sortie.WriteLine($"Téléphone : {client.Telephone}");
        _sortie.WriteLine($"E-mail : {client.Email}");
        _sortie.WriteLine("Voitures :");

        foreach (var voiture in client.Voitures.OrderBy(element => element.Marque).ThenBy(element => element.Modele))
        {
            _sortie.WriteLine($"- {voiture.Marque} {voiture.Modele} ({voiture.Annee})");
        }
    }

    private async Task AfficherFicheVoitureAsync(CancellationToken cancellationToken)
    {
        _sortie.Write("Identifiant de la voiture : ");
        var identifiant = _entree.ReadLine();

        if (!Guid.TryParse(identifiant, out var idVoiture))
        {
            _sortie.WriteLine("L'identifiant de la voiture est invalide.");
            return;
        }

        var voiture = await _contexte.Voitures
            .Include(element => element.Clients)
            .SingleOrDefaultAsync(element => element.Id == idVoiture, cancellationToken);

        if (voiture is null)
        {
            _sortie.WriteLine("Voiture introuvable.");
            return;
        }

        _sortie.WriteLine();
        _sortie.WriteLine("--- Fiche voiture ---");
        _sortie.WriteLine($"Modèle : {voiture.Marque} {voiture.Modele}");
        _sortie.WriteLine($"Année : {voiture.Annee}");
        _sortie.WriteLine($"Couleur : {voiture.Couleur}");
        _sortie.WriteLine($"Prix HT : {voiture.PrixHt.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"))} €");
        _sortie.WriteLine($"État : {(voiture.EstVendue ? "Vendue" : "Disponible")}");
        _sortie.WriteLine($"Date de vente : {voiture.DateVente?.ToString("dd/MM/yyyy") ?? "Non renseignée"}");
        _sortie.WriteLine("Client(s) :");

        foreach (var client in voiture.Clients.OrderBy(element => element.Nom).ThenBy(element => element.Prenom))
        {
            _sortie.WriteLine($"- {client.Nom} {client.Prenom} | {client.Email}");
        }
    }

    private async Task VendreVoitureAsync(CancellationToken cancellationToken)
    {
        _sortie.Write("Identifiant de la voiture à vendre : ");
        if (!Guid.TryParse(_entree.ReadLine(), out var idVoiture))
        {
            _sortie.WriteLine("L'identifiant de la voiture est invalide.");
            return;
        }

        _sortie.Write("E-mail du client : ");
        var emailClient = _entree.ReadLine();
        if (string.IsNullOrWhiteSpace(emailClient))
        {
            _sortie.WriteLine("L'e-mail est obligatoire.");
            return;
        }

        var resultat = await new ServiceVente(_contexte).VendreAsync(
            idVoiture,
            emailClient.Trim(),
            DateTime.UtcNow,
            cancellationToken);
        _sortie.WriteLine(resultat.Message);
    }

    private async Task EchangerVoitureAsync(CancellationToken cancellationToken)
    {
        _sortie.Write("Identifiant de la voiture actuelle : ");
        if (!Guid.TryParse(_entree.ReadLine(), out var idAncienneVoiture))
        {
            _sortie.WriteLine("L'identifiant de la voiture est invalide.");
            return;
        }

        _sortie.Write("Identifiant de la nouvelle voiture : ");
        if (!Guid.TryParse(_entree.ReadLine(), out var idNouvelleVoiture))
        {
            _sortie.WriteLine("L'identifiant de la voiture est invalide.");
            return;
        }

        _sortie.Write("E-mail du client : ");
        var emailClient = _entree.ReadLine();
        if (string.IsNullOrWhiteSpace(emailClient))
        {
            _sortie.WriteLine("L'e-mail est obligatoire.");
            return;
        }

        var resultat = await new ServiceVente(_contexte).EchangerAsync(
            idAncienneVoiture,
            idNouvelleVoiture,
            emailClient.Trim(),
            DateTime.UtcNow,
            cancellationToken);
        _sortie.WriteLine(resultat.Message);
    }

    private async Task AfficherChiffreAffairesHebdomadaireAsync(CancellationToken cancellationToken)
    {
        var chiffreAffaires = await new ServiceStatistiques(_contexte)
            .CalculerChiffreAffairesHebdomadaireAsync(DateTime.UtcNow, cancellationToken);
        _sortie.WriteLine(
            $"Chiffre d'affaires HT de la semaine : {chiffreAffaires.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"))} €");
    }
}
