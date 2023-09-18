using Microsoft.EntityFrameworkCore;

namespace FLS.CodeBeispiel.KatalogService.Infrastructure;

internal class KatalogService : IKatalogService
{
    private readonly KatalogContext context;

    public KatalogService(KatalogContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Guid?> GetRegionFürKundeInEinerRegionsart(Guid accountId, Guid regionsArtId)
    {
        return context.vKundenregionen
                        .Where(k => k.AccountId == accountId)
                        .Where(ra => ra.Regionsartid == regionsArtId)
                        .Select(r => r.RegionsPK)
                        .SingleOrDefaultAsync();
    }

    public Task<string> GetRegionsBezeichnung(Guid regionId)
    {
        return context.vKatalogRegion
                        .Where(r => r.RegionsID == regionId)
                        .Select(r => r.Bezeichnung)
                        .SingleAsync();
    }
}
