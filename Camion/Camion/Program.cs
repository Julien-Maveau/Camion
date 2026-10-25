using Camion.Data;
using Camion.Services;
using Microsoft.EntityFrameworkCore;

var chaineConnexion = Environment.GetEnvironmentVariable("ConnectionStrings__Garage");
if (string.IsNullOrWhiteSpace(chaineConnexion))
{
    Console.Error.WriteLine(
        "La variable d'environnement ConnectionStrings__Garage doit contenir la chaîne de connexion PostgreSQL.");
    return;
}

var repertoireDonnees = Path.Combine(AppContext.BaseDirectory, "Data");
var cheminClients = Path.Combine(repertoireDonnees, "client.csv");
var cheminVoitures = Path.Combine(repertoireDonnees, "voiture.csv");

if (!File.Exists(cheminClients) || !File.Exists(cheminVoitures))
{
    Console.Error.WriteLine("Les fichiers CSV nécessaires sont introuvables.");
    return;
}

var options = new DbContextOptionsBuilder<GarageDbContext>()
    .UseNpgsql(chaineConnexion)
    .Options;

await using var contexte = new GarageDbContext(options);
await contexte.Database.MigrateAsync();

var resultatImport = await new ImportateurCsv(contexte)
    .ImporterAsync(cheminClients, cheminVoitures);

if (resultatImport.ClientsAjoutes > 0 || resultatImport.VoituresAjoutees > 0)
{
    Console.WriteLine(
        $"Import terminé : {resultatImport.ClientsAjoutes} client(s), " +
        $"{resultatImport.VoituresAjoutees} voiture(s), " +
        $"{resultatImport.VoituresAttribuees} attribution(s).");
}

var menu = new MenuConsole(contexte, Console.In, Console.Out);
await menu.ExecuterAsync();
