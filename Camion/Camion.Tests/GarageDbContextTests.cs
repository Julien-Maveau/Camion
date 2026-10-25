using Camion.Data;
using Camion.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Camion.Tests;

public class GarageDbContextTests
{
    [Fact]
    public void La_relation_client_voiture_est_many_to_many()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase("garage-test")
            .Options;

        using var contexte = new GarageDbContext(options);

        var client = contexte.Model.FindEntityType(typeof(Client));
        var relation = client!.FindSkipNavigation(nameof(Client.Voitures));

        Assert.NotNull(relation);
        Assert.True(relation.IsCollection);
        Assert.Equal(typeof(Voiture), relation.TargetEntityType.ClrType);
    }

    [Fact]
    public void L_email_d_un_client_est_unique()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase("garage-test-email")
            .Options;

        using var contexte = new GarageDbContext(options);

        var client = contexte.Model.FindEntityType(typeof(Client));
        var indexEmail = client!.GetIndexes()
            .Single(index => index.Properties.Single().Name == nameof(Client.Email));

        Assert.True(indexEmail.IsUnique);
    }

    [Fact]
    public void Le_prix_ht_d_une_voiture_est_stocke_avec_deux_decimales()
    {
        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseInMemoryDatabase("garage-test-prix")
            .Options;

        using var contexte = new GarageDbContext(options);

        var voiture = contexte.Model.FindEntityType(typeof(Voiture));
        var prixHt = voiture!.FindProperty(nameof(Voiture.PrixHt));

        Assert.Equal(12, prixHt!.GetPrecision());
        Assert.Equal(2, prixHt.GetScale());
    }
}
