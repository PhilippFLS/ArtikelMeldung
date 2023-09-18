namespace FLS.CodeBeispiel.FunctionApp.Mapper;

public static class KundenGruppenMapper
{
    public static Models.Domain.KundenGruppe MapToDomainKgr(this CrmService.Models.CrmEntities.KundenGruppe crmKgr)
    {
        return new Models.Domain.KundenGruppe()
        {
            KgrId = crmKgr.ID,
            RegionsArt_1 = crmKgr._regionsArt_1,
            RegionsArt_2 = crmKgr._regionsArt_2,
        };
    }
}
