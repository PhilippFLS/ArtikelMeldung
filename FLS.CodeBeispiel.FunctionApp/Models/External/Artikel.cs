namespace FLS.CodeBeispiel.FunctionApp.Models.External;

public record Artikel
{
    public string? IntArtikelnummer { get; init; }

    public string ExtArtikelnummer { get; init; } = string.Empty;

    public string Bezeichnung { get; init; } = string.Empty;

    public string Gebinde { get; init; } = string.Empty;

    public string Hersteller { get; init; } = string.Empty;

    public string? FlaschenGTIN { get; init; }

    public string GebindeGTIN { get; init; } = string.Empty;

    public string? TeilmengenGTIN { get; init; }
}
