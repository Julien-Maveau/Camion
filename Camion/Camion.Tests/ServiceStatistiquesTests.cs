using Camion.Data;
using Camion.Models;
using Camion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Camion.Tests;

public class ServiceStatistiquesTests
{
    [Fact]
    public async Task CalculerChiffreAffairesHebdomadaireAsync_somme_uniquement_les_ventes_de_la_semaine()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        contexte.Voitures.AddRange(
            CreerVoiture(12000m, true, new DateTime(2026, 10, 19, 9, 0, 0, DateTimeKind.Utc)),
            CreerVoiture(15000m, true, new DateTime(2026, 10, 25, 18, 0, 0, DateTimeKind.Utc)),
            CreerVoiture(9000m, true, new DateTime(2026, 10, 18, 18, 0, 0, DateTimeKind.Utc)),
            CreerVoiture(30000m, false, null));
        await contexte.SaveChangesAsync();
        var service = new ServiceStatistiques(contexte);

        var chiffreAffaires = await service.CalculerChiffreAffairesHebdomadaireAsync(
            new DateTime(2026, 10, 25, 20, 0, 0, DateTimeKind.Utc));

        Assert.Equal(27000m, chiffreAffaires);
    }

    private static Voiture CreerVoiture(decimal prixHt, bool estVendue, DateTime? dateVente)
    {
        return new Voiture
        {
            Marque = "Renault",
            Modele = "Clio",
            Annee = 2021,
            PrixHt = prixHt,
            Couleur = "Bleu",
            EstVendue = estVendue,
            DateVente = dateVente
        };
    }
}
