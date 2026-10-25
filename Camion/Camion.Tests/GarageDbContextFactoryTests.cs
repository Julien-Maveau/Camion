using Camion.Data;
using Xunit;

namespace Camion.Tests;

public class GarageDbContextFactoryTests
{
    [Fact]
    public void La_fabrique_de_conception_utilise_le_fournisseur_postgresql()
    {
        var fabrique = new GarageDbContextFactory();

        using var contexte = fabrique.CreateDbContext([]);

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", contexte.Database.ProviderName);
    }
}
