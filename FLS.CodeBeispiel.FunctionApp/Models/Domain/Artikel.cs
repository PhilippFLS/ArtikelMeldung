namespace FLS.CodeBeispiel.FunctionApp.Models.Domain;

/// <summary>
/// Artikeldaten, welche in einer Meldung vorkommen
/// </summary>
public record Artikel
{
    public string? IntNr { get; init; }
    public string ExtArtikelnummer { get; init; } = string.Empty;
    public string Bezeichnung { get; init; } = string.Empty;
    public string Gebinde { get; init; } = string.Empty;
    public string Hersteller { get; init; } = string.Empty;

    public string? FlaschenEAN { get; init; }

    /// <summary>
    /// Muss als einzige EAN zwingend vorhanden sein
    /// </summary>
    public string GebindeEAN { get; init; } = string.Empty;

    public string? TeilmengenEAN { get; init; }
}
