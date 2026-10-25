namespace Camion.Models;

/// <summary>
/// Représente une voiture vendue ou disponible dans le garage.
/// </summary>
public class Voiture
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Marque { get; set; } = string.Empty;

    public string Modele { get; set; } = string.Empty;

    public int Annee { get; set; }

    // decimal est adapté aux montants financiers.
    public decimal PrixHt { get; set; }

    public string Couleur { get; set; } = string.Empty;

    public bool EstVendue { get; set; }

    public DateTime? DateVente { get; set; }

    /// <summary>
    /// Clients liés à la voiture par la relation plusieurs-à-plusieurs configurée dans Entity Framework Core.
    /// </summary>
    public ICollection<Client> Clients { get; } = new List<Client>();
}
