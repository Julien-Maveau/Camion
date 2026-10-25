using Camion.Data;
using Camion.Models;
using Camion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Camion.Tests;

public class ServiceVenteTests
{
    [Fact]
    public async Task VendreAsync_vend_une_voiture_disponible_a_un_client()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        var client = CreerClient("lea.martin@example.com");
        var voiture = CreerVoiture("Clio");
        contexte.AddRange(client, voiture);
        await contexte.SaveChangesAsync();
        var service = new ServiceVente(contexte);
        var dateVente = new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc);

        var resultat = await service.VendreAsync(voiture.Id, client.Email, dateVente);

        Assert.True(resultat.Succes);
        Assert.True(voiture.EstVendue);
        Assert.Equal(dateVente, voiture.DateVente);
        Assert.Contains(client, voiture.Clients);
    }

    [Fact]
    public async Task VendreAsync_refuse_la_vente_si_le_client_possede_deja_deux_voitures()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        var client = CreerClient("lea.martin@example.com");
        var premiereVoiture = CreerVoiture("Clio");
        premiereVoiture.EstVendue = true;
        premiereVoiture.Clients.Add(client);
        var deuxiemeVoiture = CreerVoiture("Mégane");
        deuxiemeVoiture.EstVendue = true;
        deuxiemeVoiture.Clients.Add(client);
        var voitureDisponible = CreerVoiture("Captur");
        contexte.AddRange(premiereVoiture, deuxiemeVoiture, voitureDisponible);
        await contexte.SaveChangesAsync();
        var service = new ServiceVente(contexte);

        var resultat = await service.VendreAsync(
            voitureDisponible.Id,
            client.Email,
            new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc));

        Assert.False(resultat.Succes);
        Assert.Contains("deux voitures", resultat.Message);
        Assert.False(voitureDisponible.EstVendue);
        Assert.Empty(voitureDisponible.Clients);
    }

    [Fact]
    public async Task EchangerAsync_remplace_la_voiture_vendue_du_client_par_une_voiture_disponible()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        var client = CreerClient("lea.martin@example.com");
        var ancienneVoiture = CreerVoiture("Clio");
        ancienneVoiture.EstVendue = true;
        ancienneVoiture.DateVente = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);
        ancienneVoiture.Clients.Add(client);
        var nouvelleVoiture = CreerVoiture("Captur");
        contexte.AddRange(ancienneVoiture, nouvelleVoiture);
        await contexte.SaveChangesAsync();
        var service = new ServiceVente(contexte);
        var dateEchange = new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc);

        var resultat = await service.EchangerAsync(
            ancienneVoiture.Id,
            nouvelleVoiture.Id,
            client.Email,
            dateEchange);

        Assert.True(resultat.Succes);
        Assert.False(ancienneVoiture.EstVendue);
        Assert.Null(ancienneVoiture.DateVente);
        Assert.Empty(ancienneVoiture.Clients);
        Assert.True(nouvelleVoiture.EstVendue);
        Assert.Equal(dateEchange, nouvelleVoiture.DateVente);
        Assert.Contains(client, nouvelleVoiture.Clients);
    }

    private static Client CreerClient(string email)
    {
        return new Client
        {
            Nom = "Martin",
            Prenom = "Léa",
            DateNaissance = new DateTime(1994, 3, 18),
            Telephone = "0612345678",
            Email = email
        };
    }

    private static Voiture CreerVoiture(string modele)
    {
        return new Voiture
        {
            Marque = "Renault",
            Modele = modele,
            Annee = 2021,
            PrixHt = 12500m,
            Couleur = "Bleu",
            EstVendue = false
        };
    }
}
