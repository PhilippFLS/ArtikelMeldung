namespace FLS.CodeBeispiel.FunctionApp.Mapper;

public static class AccountMapper
{
    public static Models.Domain.Account MapToDomainAccount(this CrmService.Models.CrmEntities.Account crmAccount)
    {
        return new Models.Domain.Account()
        {
            CrmAccountId = crmAccount.Accountid,
            KgrId = crmAccount._kgr_id ?? Guid.Empty,
        };
    }
}
