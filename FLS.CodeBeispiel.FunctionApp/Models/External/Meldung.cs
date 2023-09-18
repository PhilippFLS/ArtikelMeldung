namespace FLS.CodeBeispiel.FunctionApp.Models.External;

public record Meldung
{
    public string ExtPartner { get; init; } = string.Empty;

    public Guid ExtPartnerCrmId { get; init; }

    public int? ExtPartnerNr { get; set; }

    public IEnumerable<Artikel> Artikel { get; init; } = Array.Empty<Artikel>();

    public IEnumerable<Preisgruppe> Preisgruppe { get; init; } = Array.Empty<Preisgruppe>();
}
