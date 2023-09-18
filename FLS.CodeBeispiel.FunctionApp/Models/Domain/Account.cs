namespace FLS.CodeBeispiel.FunctionApp.Models.Domain;

public record Account
{
    public Guid CrmAccountId { get; init; }
    public Guid KgrId { get; init; }
}
