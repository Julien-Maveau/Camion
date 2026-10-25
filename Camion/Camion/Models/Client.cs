namespace Camion.Models;

/// <summary>
/// Représente un client du garage.
/// </summary>
public class Client
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nom { get; set; } = string.Empty;

    public string Prenom { get; set; } = string.Empty;

    public DateTime DateNaissance { get; set; }

    // Le téléphone est une chaîne pour conserver les zéros initiaux.
    public string Telephone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Voitures possédées par le client. La limite de deux sera vérifiée
    /// par le service de vente.
    /// </summary>
    public ICollection<Voiture> Voitures { get; } = new List<Voiture>();

    public int CalculerAge(DateTime dateReference)
    {
        return Calculateur.CalculerAge(DateNaissance, dateReference);
    }
}
