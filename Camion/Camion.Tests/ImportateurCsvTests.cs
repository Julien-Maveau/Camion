using Camion.Data;
using Camion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Camion.Tests;

public class ImportateurCsvTests
{
    [Fact]
    public async Task ImporterAsync_attribue_au_maximum_deux_voitures_vendues_par_client()
    {
        var repertoire = CreerRepertoireDonnees(
            """
            Nom;Prénom;Date de naissance;Téléphone;Email
            Martin;Léa;1994-03-18;0612345678;lea.martin@example.com
            """,
            """
            Marque$Modele$Annee$Prix_HT$Couleur$Vendue
            Renault$Clio$2021$12500.00$Bleu$true
            Peugeot$208$2022$14800.00$Gris$true
            Citroën$C3$2020$10900.00$Blanc$true
            Toyota$Yaris$2023$16800.00$Rouge$false
            """);

        try
        {
            var options = new DbContextOptionsBuilder<GarageDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var contexte = new GarageDbContext(options);
            var importateur = new ImportateurCsv(contexte);

            var resultat = await importateur.ImporterAsync(
                Path.Combine(repertoire, "client.csv"),
                Path.Combine(repertoire, "voiture.csv"));

            var client = await contexte.Clients
                .Include(element => element.Voitures)
                .SingleAsync();
            var voituresVendues = await contexte.Voitures
                .Where(element => element.EstVendue)
                .ToListAsync();

            Assert.Equal(1, resultat.ClientsAjoutes);
            Assert.Equal(4, resultat.VoituresAjoutees);
            Assert.Equal(2, resultat.VoituresAttribuees);
            Assert.Equal(2, client.Voitures.Count);
            Assert.Equal(2, voituresVendues.Count(element => element.Clients.Count == 1));
            Assert.Equal(1, voituresVendues.Count(element => element.Clients.Count == 0));
        }
        finally
        {
            Directory.Delete(repertoire, true);
        }
    }

    [Fact]
    public async Task ImporterAsync_ne_cree_pas_de_doublon_lors_d_un_second_import()
    {
        var repertoire = CreerRepertoireDonnees(
            """
            Nom;Prénom;Date de naissance;Téléphone;Email
            Martin;Léa;1994-03-18;0612345678;lea.martin@example.com
            Bernard;Thomas;1987-11-02;0623456789;thomas.bernard@example.com
            """,
            """
            Marque$Modele$Annee$Prix_HT$Couleur$Vendue
            Renault$Clio$2021$12500.00$Bleu$true
            Citroën$C3$2020$10900.00$Blanc$false
            """);

        try
        {
            var options = new DbContextOptionsBuilder<GarageDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var contexte = new GarageDbContext(options);
            var importateur = new ImportateurCsv(contexte);

            await importateur.ImporterAsync(
                Path.Combine(repertoire, "client.csv"),
                Path.Combine(repertoire, "voiture.csv"));
            var secondResultat = await importateur.ImporterAsync(
                Path.Combine(repertoire, "client.csv"),
                Path.Combine(repertoire, "voiture.csv"));

            Assert.Equal(0, secondResultat.ClientsAjoutes);
            Assert.Equal(0, secondResultat.VoituresAjoutees);
            Assert.Equal(0, secondResultat.VoituresAttribuees);
            Assert.Equal(2, await contexte.Clients.CountAsync());
            Assert.Equal(2, await contexte.Voitures.CountAsync());
        }
        finally
        {
            Directory.Delete(repertoire, true);
        }
    }

    [Fact]
    public async Task ImporterAsync_importe_les_fichiers_fournis_avec_le_projet()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        var importateur = new ImportateurCsv(contexte);
        var repertoire = Path.Combine(AppContext.BaseDirectory, "Data");

        var resultat = await importateur.ImporterAsync(
            Path.Combine(repertoire, "client.csv"),
            Path.Combine(repertoire, "voiture.csv"));
        var clients = await contexte.Clients
            .Include(client => client.Voitures)
            .ToListAsync();
        var voituresVendues = await contexte.Voitures
            .Where(voiture => voiture.EstVendue)
            .Include(voiture => voiture.Clients)
            .ToListAsync();

        Assert.Equal(10, resultat.ClientsAjoutes);
        Assert.Equal(12, resultat.VoituresAjoutees);
        Assert.Equal(7, resultat.VoituresAttribuees);
        Assert.Equal(10, clients.Count);
        Assert.Equal(12, await contexte.Voitures.CountAsync());
        Assert.All(clients, client => Assert.InRange(client.Voitures.Count, 0, 2));
        Assert.Equal(7, voituresVendues.Count);
        Assert.All(voituresVendues, voiture => Assert.Single(voiture.Clients));
    }

    private static string CreerRepertoireDonnees(string clients, string voitures)
    {
        var repertoire = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(repertoire);
        File.WriteAllText(Path.Combine(repertoire, "client.csv"), clients);
        File.WriteAllText(Path.Combine(repertoire, "voiture.csv"), voitures);
        return repertoire;
    }
}
