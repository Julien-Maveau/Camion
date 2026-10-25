using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Camion.Data;

/// <summary>
/// Crée le contexte pour les commandes Entity Framework, notamment les migrations.
/// </summary>
public class GarageDbContextFactory : IDesignTimeDbContextFactory<GarageDbContext>
{
    public GarageDbContext CreateDbContext(string[] args)
    {
        var chaineConnexion = Environment.GetEnvironmentVariable("ConnectionStrings__Garage")
            ?? "Host=localhost;Port=5432;Database=camion;Username=camion;Password=mot-de-passe-non-utilise";

        var options = new DbContextOptionsBuilder<GarageDbContext>()
            .UseNpgsql(chaineConnexion)
            .Options;

        return new GarageDbContext(options);
    }
}
