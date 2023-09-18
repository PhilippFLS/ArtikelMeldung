namespace FLS.CodeBeispiel.KatalogService.Infrastructure;

public interface IKatalogService
{
    Task<Guid?> GetRegionFürKundeInEinerRegionsart(Guid accountId, Guid regionsArtId);

    Task<string>GetRegionsBezeichnung(Guid regionId);
}
