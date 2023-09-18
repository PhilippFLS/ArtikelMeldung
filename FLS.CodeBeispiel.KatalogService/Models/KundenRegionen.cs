namespace FLS.CodeBeispiel.KatalogService.Models;

public record KundenRegionen
{
    public Guid AccountId { get; init; }
    public Guid? RegionsPK { get; init; }
    public Guid? Regionsartid { get; init; }
    public string? RegionsNummer { get; init; }
    public string? RegionsArt { get; init; }
}
