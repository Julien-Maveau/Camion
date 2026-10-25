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
        var voiture = new Voiture
        {
            Marque = "Renault",
            Modele = "Clio",
            Annee = 2021,
            PrixHt = 12500m,
            Couleur = "Bleu",
            EstVendue = false
        };
        contexte.Voitures.Add(voiture);
        await contexte.SaveChangesAsync();

        using var entree = new StringReader("2\n0\n");
        using var sortie = new StringWriter();
        var menu = new MenuConsole(contexte, entree, sortie);

        await menu.ExecuterAsync();

        var texte = sortie.ToString();
        Assert.Contains("Liste des voitures", texte);
        Assert.Contains("Renault Clio", texte);
        Assert.Contains(voiture.Id.ToString(), texte);
        Assert.Contains("Disponible", texte);
    }

    [Fact]
    public async Task ExecuterAsync_affiche_la_fiche_d_un_client_et_ses_voitures()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        var voiture = new Voiture
        {
            Marque = "Renault",
            Modele = "Clio",
            Annee = 2021,
            PrixHt = 12500m,
            Couleur = "Bleu",
            EstVendue = true
        };
        var client = new Client
        {
            Nom = "Martin",
            Prenom = "Léa",
            DateNaissance = new DateTime(1994, 3, 18),
            Telephone = "0612345678",
            Email = "lea.martin@example.com"
        };
        client.Voitures.Add(voiture);
        contexte.Clients.Add(client);
        await contexte.SaveChangesAsync();

        using var entree = new StringReader("3\nlea.martin@example.com\n0\n");
        using var sortie = new StringWriter();
        var menu = new MenuConsole(contexte, entree, sortie);

        await menu.ExecuterAsync();

        var texte = sortie.ToString();
        Assert.Contains("Fiche client", texte);
        Assert.Contains("Martin Léa", texte);
        Assert.Contains("0612345678", texte);
        Assert.Contains("Renault Clio", texte);
    }

    [Fact]
    public async Task ExecuterAsync_affiche_la_fiche_d_une_voiture_et_son_client()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        var voiture = new Voiture
        {
            Marque = "Renault",
            Modele = "Clio",
            Annee = 2021,
            PrixHt = 12500m,
            Couleur = "Bleu",
            EstVendue = true,
            DateVente = new DateTime(2026, 10, 25)
        };
        var client = new Client
        {
            Nom = "Martin",
            Prenom = "Léa",
            DateNaissance = new DateTime(1994, 3, 18),
            Telephone = "0612345678",
            Email = "lea.martin@example.com"
        };
        voiture.Clients.Add(client);
        contexte.Voitures.Add(voiture);
        await contexte.SaveChangesAsync();

        using var entree = new StringReader($"4\n{voiture.Id}\n0\n");
        using var sortie = new StringWriter();
        var menu = new MenuConsole(contexte, entree, sortie);

        await menu.ExecuterAsync();

        var texte = sortie.ToString();
        Assert.Contains("Fiche voiture", texte);
        Assert.Contains("Renault Clio", texte);
        Assert.Contains("Bleu", texte);
        Assert.Contains("Martin Léa", texte);
    }

    [Fact]
    public async Task ExecuterAsync_vend_une_voiture_disponible()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        var client = new Client
        {
            Nom = "Martin",
            Prenom = "Léa",
            DateNaissance = new DateTime(1994, 3, 18),
            Telephone = "0612345678",
            Email = "lea.martin@example.com"
        };
        var voiture = new Voiture
        {
            Marque = "Renault",
            Modele = "Clio",
            Annee = 2021,
            PrixHt = 12500m,
            Couleur = "Bleu",
            EstVendue = false
        };
        contexte.AddRange(client, voiture);
        await contexte.SaveChangesAsync();

        using var entree = new StringReader($"5\n{voiture.Id}\n{client.Email}\n0\n");
        using var sortie = new StringWriter();
        var menu = new MenuConsole(contexte, entree, sortie);

        await menu.ExecuterAsync();

        Assert.True(voiture.EstVendue);
        Assert.Contains(client, voiture.Clients);
        Assert.Contains("La voiture a été vendue.", sortie.ToString());
    }

    [Fact]
    public async Task ExecuterAsync_quitte_proprement_lorsque_l_entree_est_terminee()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexte = new GarageDbContext(options);
        using var entree = new StringReader(string.Empty);
        using var sortie = new StringWriter();
        var menu = new MenuConsole(contexte, entree, sortie);

        await menu.ExecuterAsync();

        Assert.Contains("Au revoir.", sortie.ToString());
    }
}
