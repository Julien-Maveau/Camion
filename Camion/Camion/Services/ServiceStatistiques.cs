using Camion.Data;
using Microsoft.EntityFrameworkCore;

namespace Camion.Services;

/// <summary>
/// Calcule les indicateurs de vente du garage.
/// </summary>
public class ServiceStatistiques
{
    private readonly GarageDbContext _contexte;

    public ServiceStatistiques(GarageDbContext contexte)
    {
        _contexte = contexte;
    }

    /// <summary>
    /// Calcule le chiffre d'affaires HT depuis le lundi de la semaine de référence jusqu'à cette date.
    /// </summary>
    public async Task<decimal> CalculerChiffreAffairesHebdomadaireAsync(
        DateTime dateReference,
        CancellationToken cancellationToken = default)
    {
        var decalageDepuisLundi = ((int)dateReference.DayOfWeek + 6) % 7;
        var debutSemaine = dateReference.Date.AddDays(-decalageDepuisLundi);

        return await _contexte.Voitures
            .Where(element =>
                element.EstVendue &&
                element.DateVente.HasValue &&
                element.DateVente.Value >= debutSemaine &&
                element.DateVente.Value <= dateReference)
            .SumAsync(element => (decimal?)element.PrixHt, cancellationToken) ?? 0m;
    }
}
