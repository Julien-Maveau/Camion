using Camion.Models;
using Xunit;

namespace Camion.Tests;

public class ClientTests
{
    [Fact]
    public void Un_client_nouveau_ne_possede_aucune_voiture()
    {
        var client = new Client
        {
            Nom = "Martin",
            Prenom = "Léa",
            DateNaissance = new DateTime(1994, 3, 18),
            Telephone = "0612345678",
            Email = "lea.martin@example.com"
        };

        Assert.Empty(client.Voitures);
    }

    [Fact]
    public void CalculerAge_reduit_l_age_avant_l_anniversaire()
    {
        var client = new Client
        {
            DateNaissance = new DateTime(2000, 6, 15)
        };

        var age = client.CalculerAge(new DateTime(2026, 6, 14));

        Assert.Equal(25, age);
    }
}
