using Camion.Models;
using Microsoft.EntityFrameworkCore;

namespace Camion.Data;

/// <summary>
/// Représente la base de données du garage.
/// </summary>
public class GarageDbContext : DbContext
{
    public GarageDbContext(DbContextOptions<GarageDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Voiture> Voitures => Set<Voiture>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>()
            .HasIndex(client => client.Email)
            .IsUnique();

        modelBuilder.Entity<Voiture>()
            .Property(voiture => voiture.PrixHt)
            .HasPrecision(12, 2);

        modelBuilder.Entity<Client>()
            .HasMany(client => client.Voitures)
            .WithMany(voiture => voiture.Clients)
            .UsingEntity(relation => relation.ToTable("ClientVoitures"));
    }
}
