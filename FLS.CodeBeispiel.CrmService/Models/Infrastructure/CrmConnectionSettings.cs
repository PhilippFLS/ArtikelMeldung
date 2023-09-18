namespace FLS.CodeBeispiel.CrmService.Models.Infrastructure;

public record CrmConnectionSettings
{
    /// <summary>
    /// Das Secret des verwendeten Clients
    /// </summary>
    public string ClientSecret { get; init; } = string.Empty;
    /// <summary>
    /// Die ID des verwendeten Clients, der im CRM angelegt
    /// und im AAD des TBCon-Tenants bekannt ist
    /// </summary>
    public string ClientID { get; init; } = string.Empty;
    /// <summary>
    /// URL des CRMs
    /// </summary>
    public Uri URL { get; init; }
    /// <summary>
    /// Der Tenant, in dem der Client bekannt ist. I.d.R TBCon-Tenant
    /// </summary>
    public string TenantID { get; init; } = string.Empty;
    /// <summary>
    /// Scope, der sich standardmäßig aus der URL des CRM und dem Suffix /.default zusammensetzt
    /// </summary>
    public string Scope { get; init; } = string.Empty;
}
