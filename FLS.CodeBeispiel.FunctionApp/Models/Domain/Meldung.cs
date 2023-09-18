namespace FLS.CodeBeispiel.FunctionApp.Models.Domain;

public class Meldung
{
    public Partner Partner { get; init; } = new();

    public IEnumerable<Artikel> Artikel { get; init; } = Array.Empty<Artikel>();
    public List<Katalog> Katalog { get; init; } = new List<Katalog>();
}
