using Camion.Data;
using Camion.Models;
using Camion.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Camion.Tests;

public class MenuConsoleTests
{
    [Fact]
    public async Task ExecuterAsync_affiche_la_liste_des_clients()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        contexte.Clients.Add(new Client
        {
            Nom = "Martin",
            Prenom = "Léa",
            DateNaissance = new DateTime(1994, 3, 18),
            Telephone = "0612345678",
            Email = "lea.martin@example.com"
        });
        await contexte.SaveChangesAsync();

        using var entree = new StringReader("1\n0\n");
        using var sortie = new StringWriter();
        var menu = new MenuConsole(contexte, entree, sortie);

        await menu.ExecuterAsync();

        var texte = sortie.ToString();
        Assert.Contains("Liste des clients", texte);
        Assert.Contains("Martin Léa", texte);
        Assert.Contains("lea.martin@example.com", texte);
    }

    [Fact]
    public async Task ExecuterAsync_affiche_la_liste_des_voitures()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        contexte.Voitures.Add(new Voiture
        {
            Marque = "Renault",
            Modele = "Clio",
            Annee = 2021,
            PrixHt = 12500m,
            Couleur = "Bleu",
            EstVendue = false
        });
        await contexte.SaveChangesAsync();

        using var entree = new StringReader("2\n0\n");
        using var sortie = new StringWriter();
        var menu = new MenuConsole(contexte, entree, sortie);

        await menu.ExecuterAsync();

        var texte = sortie.ToString();
        Assert.Contains("Liste des voitures", texte);
        Assert.Contains("Renault Clio", texte);
        Assert.Contains("Disponible", texte);
    }
}
