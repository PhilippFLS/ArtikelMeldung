namespace FLS.CodeBeispiel.FunctionApp.Mapper;

public static class KgrMapper
{
    public static Models.Domain.Kgr MapToDomainKgr(this CrmService.Models.CrmEntities.KundenGr crmKgr)
    {
        return new Models.Domain.Kgr()
        {
            KgrId = crmKgr.ID,
            RegionsArt_1 = crmKgr._regionsArt_1,
            RegionsArt_2 = crmKgr._regionsArt_2,
        };
    }
}
