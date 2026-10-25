using System.Globalization;
using Camion.Data;
using Camion.Models;
using Microsoft.EntityFrameworkCore;

namespace Camion.Services;

/// <summary>
/// Importe les fichiers CSV fournis avec le projet dans la base du garage.
/// </summary>
public class ImportateurCsv
{
    private readonly GarageDbContext _contexte;

    public ImportateurCsv(GarageDbContext contexte)
    {
        _contexte = contexte;
    }

    public async Task<ResultatImportation> ImporterAsync(
        string cheminClients,
        string cheminVoitures,
        CancellationToken cancellationToken = default)
    {
        var clientsAjoutes = await ImporterClientsAsync(cheminClients, cancellationToken);
        var voituresAjoutees = await ImporterVoituresAsync(cheminVoitures, cancellationToken);
        var voituresAttribuees = await AttribuerVoituresVenduesAsync(cancellationToken);

        return new ResultatImportation(clientsAjoutes, voituresAjoutees, voituresAttribuees);
    }

    private async Task<int> ImporterClientsAsync(string chemin, CancellationToken cancellationToken)
    {
        var clientsAjoutes = 0;

        foreach (var ligne in LireLignesDonnees(chemin))
        {
            var colonnes = ligne.Split(';');
            VerifierNombreColonnes(chemin, colonnes, 5);
            var email = colonnes[4].Trim();

            if (await _contexte.Clients.AnyAsync(client => client.Email == email, cancellationToken))
            {
                continue;
            }

            _contexte.Clients.Add(new Client
            {
                Nom = colonnes[0].Trim(),
                Prenom = colonnes[1].Trim(),
                DateNaissance = LireDate(colonnes[2], chemin),
                Telephone = colonnes[3].Trim(),
                Email = email
            });
            clientsAjoutes++;
        }

        await _contexte.SaveChangesAsync(cancellationToken);
        return clientsAjoutes;
    }

    private async Task<int> ImporterVoituresAsync(string chemin, CancellationToken cancellationToken)
    {
        var voituresAjoutees = 0;

        foreach (var ligne in LireLignesDonnees(chemin))
        {
            var colonnes = ligne.Split('$');
            VerifierNombreColonnes(chemin, colonnes, 6);

            var marque = colonnes[0].Trim();
            var modele = colonnes[1].Trim();
            var annee = LireEntier(colonnes[2], chemin);
            var prixHt = LireDecimal(colonnes[3], chemin);
            var couleur = colonnes[4].Trim();
            var estVendue = LireBooleen(colonnes[5], chemin);

            var existe = await _contexte.Voitures.AnyAsync(
                voiture => voiture.Marque == marque
                    && voiture.Modele == modele
                    && voiture.Annee == annee
                    && voiture.PrixHt == prixHt
                    && voiture.Couleur == couleur,
                cancellationToken);

            if (existe)
            {
                continue;
            }

            _contexte.Voitures.Add(new Voiture
            {
                Marque = marque,
                Modele = modele,
                Annee = annee,
                PrixHt = prixHt,
                Couleur = couleur,
                EstVendue = estVendue
            });
            voituresAjoutees++;
        }

        await _contexte.SaveChangesAsync(cancellationToken);
        return voituresAjoutees;
    }

    /// <summary>
    /// Attribue les voitures déjà vendues de façon déterministe : voitures triées par marque et modèle,
    /// puis clients triés par e-mail, sans dépasser deux voitures par client.
    /// </summary>
    private async Task<int> AttribuerVoituresVenduesAsync(CancellationToken cancellationToken)
    {
        var clients = await _contexte.Clients
            .Include(client => client.Voitures)
            .OrderBy(client => client.Email)
            .ToListAsync(cancellationToken);
        var voituresVendues = await _contexte.Voitures
            .Where(voiture => voiture.EstVendue)
            .Include(voiture => voiture.Clients)
            .OrderBy(voiture => voiture.Marque)
            .ThenBy(voiture => voiture.Modele)
            .ToListAsync(cancellationToken);

        var voituresAttribuees = 0;
        foreach (var voiture in voituresVendues)
        {
            if (voiture.Clients.Count > 0)
            {
                continue;
            }

            var client = clients.FirstOrDefault(element => element.Voitures.Count < 2);
            if (client is null)
            {
                break;
            }

            client.Voitures.Add(voiture);
            voituresAttribuees++;
        }

        await _contexte.SaveChangesAsync(cancellationToken);
        return voituresAttribuees;
    }

    private static IEnumerable<string> LireLignesDonnees(string chemin)
    {
        return File.ReadLines(chemin)
            .Skip(1)
            .Where(ligne => !string.IsNullOrWhiteSpace(ligne));
    }

    private static void VerifierNombreColonnes(string chemin, string[] colonnes, int nombreAttendu)
    {
        if (colonnes.Length != nombreAttendu)
        {
            throw new FormatException($"Le fichier '{chemin}' contient une ligne avec un nombre de colonnes invalide.");
        }
    }

    private static DateTime LireDate(string valeur, string chemin)
    {
        if (DateTime.TryParseExact(
                valeur.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var date))
        {
            return date;
        }

        throw new FormatException($"La date '{valeur}' du fichier '{chemin}' est invalide.");
    }

    private static int LireEntier(string valeur, string chemin)
    {
        if (int.TryParse(valeur.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var entier))
        {
            return entier;
        }

        throw new FormatException($"Le nombre entier '{valeur}' du fichier '{chemin}' est invalide.");
    }

    private static decimal LireDecimal(string valeur, string chemin)
    {
        if (decimal.TryParse(valeur.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalLu))
        {
            return decimalLu;
        }

        throw new FormatException($"Le montant '{valeur}' du fichier '{chemin}' est invalide.");
    }

    private static bool LireBooleen(string valeur, string chemin)
    {
        if (bool.TryParse(valeur.Trim(), out var booleen))
        {
            return booleen;
        }

        throw new FormatException($"La valeur booléenne '{valeur}' du fichier '{chemin}' est invalide.");
    }
}

public record ResultatImportation(int ClientsAjoutes, int VoituresAjoutees, int VoituresAttribuees);
