using Camion.Models;
using Xunit;

namespace Camion.Tests;

public class VoitureTests
{
    [Fact]
    public void Une_voiture_nouvelle_est_disponible_et_ne_possede_aucun_client()
    {
        var voiture = new Voiture
        {
            Marque = "Renault",
            Modele = "Clio",
            Annee = 2021,
            PrixHt = 12500.00m,
            Couleur = "Bleu"
        };

        Assert.False(voiture.EstVendue);
        Assert.Null(voiture.DateVente);
        Assert.Empty(voiture.Clients);
        Assert.Equal(12500.00m, voiture.PrixHt);
    }
}
