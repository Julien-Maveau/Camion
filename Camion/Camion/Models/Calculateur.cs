namespace Camion.Models;

/// <summary>
/// Contient les calculs métier réutilisables du garage.
/// </summary>
public static class Calculateur
{
    public static int CalculerAge(DateTime dateNaissance, DateTime dateReference)
    {
        var age = dateReference.Year - dateNaissance.Year;

        if (dateNaissance.Date > dateReference.AddYears(-age).Date)
        {
            age--;
        }

        return age;
    }
}
