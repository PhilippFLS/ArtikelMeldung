using Microsoft.Extensions.Logging;
using FLS.CodeBeispiel.CrmService.Enumerations;
using FLS.CodeBeispiel.CrmService.Extensions;
using FLS.CodeBeispiel.CrmService.Models.CrmEntities;
using FLS.CodeBeispiel.CrmService.Models.Infrastructure;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Identity.Client;

[assembly: InternalsVisibleTo("FLS.CodeBeispiel.CrmService.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace FLS.CodeBeispiel.CrmService.Infrastructure;

/// <summary>
/// Service, über welchen die Kommunikation mit dem CRM implementiert ist
/// </summary>
internal class CrmService : ICrmService
{
    private readonly ILogger<CrmService> logger;
    private readonly HttpClient httpClient;

    public CrmService(HttpClient httpClient, ILogger<CrmService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<int?> GetNummerForAccount(Guid accountId)
    {
        var accountFilter = new ODataFilter<Guid>()
        {
            Key = nameof(Account.Accountid).ToLower(),
            Value = accountId,
        };


        var uri = new UriBuilder(httpClient.BaseAddress!.ToString())
                                  .SetBaseEntity(CrmTableNames.Account)
                                  .WithFilter(accountFilter)
                                  .WithSelect(nameof(Account.Accountnumber).ToLower())
                                  .BuildUri();

        var account = await GetHttpResponseAsync<Account>(uri);

        if (int.TryParse(account?.Value.First().Accountnumber, out int sapNr))
        {
            return sapNr;
        }
        else
        {
            logger.LogWarning("Für den Account mit der ID {accountId} konnte keine Nummer ermittelt werden.", accountId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Account>> GetAlleKundenFürPartner(Guid partnerAccountId)
    {
        var customFilter = new ODataFilter<string>()
        {
            Key = $"({nameof(Account.Hauptabrechner)} {ODataOperators.eq} {partnerAccountId} or {nameof(Account.Nebenabrechner)} {ODataOperators.eq} {partnerAccountId})",
        };

        var aktiveKundenFilter = new ODataFilter<int?>()
        {
            Key = nameof(Account.Statuscode).ToLower(),
            Value = 0,
        };

        var zugeordneteAccountsUri = new UriBuilder(httpClient.BaseAddress!.ToString())
                                      .SetBaseEntity(CrmTableNames.Account)
                                      .WithCustomFilter(customFilter)
                                      .WithFilter(aktiveKundenFilter)
                                      .WithSelect(nameof(Account.Accountid).ToLower())
                                      .WithSelect(nameof(Account.Accountnumber).ToLower())
                                      .WithSelect(nameof(Account._kgr_id))
                                      .WithExpand(nameof(Account.K_Gr).ToLower(), new string[] { nameof(KundenGruppe._regionsArt_1), nameof(KundenGruppe._regionsArt_2), nameof(KundenGruppe._pgr_id) })
                                      .BuildUri();

        var zugeordneteAccounts = await GetHttpResponseAsync<Account>(zugeordneteAccountsUri);

        return zugeordneteAccounts?.Value ?? Array.Empty<Account>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<KundenGruppe>> GetAlleKundengruppenFürPreisgruppen(int preisgruppenNummer)
    {
        var pgrGuid = await GetPreisgruppenGuid(preisgruppenNummer);

        if (pgrGuid is null)
        {
            logger.LogWarning("Für die Preisgruppe mit der Nummer {preisgruppenNummer} gibt es keinen Eintrag im CRM.", preisgruppenNummer);
            return Array.Empty<KundenGruppe>();
        }

        return await GetZugeordneteKundenGruppenFürPreisGruppe((Guid)pgrGuid);
    }

    /// <summary>
    /// Ermittelt für eine PreisgruppenNummer die ID der Preisgruppe im CRM
    /// </summary>
    /// <param name="pgrNummer">Die zugeordnete Preisgruppennummer einer Preisgruppe</param>
    /// <returns>Die ID der Preisgruppe</returns>
    private async Task<Guid?> GetPreisgruppenGuid(int pgrNummer)
    {
        var pgrFilter = new ODataFilter<int?>()
        {
            Key = nameof(PreisGruppe.Nummer).ToLower(),
            Value = pgrNummer
        };

        var pgrUri = new UriBuilder(httpClient.BaseAddress!.ToString())
                                .SetBaseEntity(CrmTableNames.PreisGr)
                                .WithFilter(pgrFilter)
                                .WithSelect(nameof(PreisGruppe.Id).ToLower())
                                .BuildUri();

        var pgr = await GetHttpResponseAsync<PreisGruppe>(pgrUri);

        return pgr != null && pgr.Value.Any() ? pgr.Value[0].Id : null;
    }

    /// <summary>
    /// Ermittelt für eine angegebene Preisgruppen alle Kundengruppen, die die Preisgruppe gesetzt haben
    /// </summary>
    /// <param name="pgrGuid">Die CRM-ID der Preisgruppe</param>
    /// <returns>Liste von allen Kundengruppen, die der Preisgruppe zugeordnet sind</returns>
    private async Task<IEnumerable<KundenGruppe>> GetZugeordneteKundenGruppenFürPreisGruppe(Guid pgrGuid)
    {
        var kgrFilter = new ODataFilter<Guid>()
        {
            Key = nameof(KundenGruppe._pgr_id),
            Value = pgrGuid
        };

        var kgrUri = new UriBuilder(httpClient.BaseAddress!.ToString())
                                .SetBaseEntity(CrmTableNames.KundenGr)
                                .WithFilter(kgrFilter)
                                .WithSelect(nameof(KundenGruppe.ID).ToLower())
                                .WithSelect(nameof(KundenGruppe._regionsArt_1))
                                .WithSelect(nameof(KundenGruppe._regionsArt_2))
                                .BuildUri();

        var kgrs = await GetHttpResponseAsync<KundenGruppe>(kgrUri);

        return kgrs?.Value ?? Array.Empty<KundenGruppe>();
    }

    /// <summary>
    /// Hilfsfunktion zum Senden und Empfangen der HTTP-Request und Deserialisierung der Response
    /// </summary>
    /// <typeparam name="TValue">Zu deserialisierender Typ</typeparam>
    /// <param name="uri">URI, dessen Request deserialisiert werden soll</param>
    /// <returns>Deserialisierte Antwort des CRMs</returns>
    private async Task<CrmBaseResponse<TValue>?> GetHttpResponseAsync<TValue>(Uri uri)
    {
        var crmReponse = await httpClient.GetAsync(uri);

        return JsonSerializer.Deserialize<CrmBaseResponse<TValue>>(await crmReponse.Content.ReadAsStringAsync(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
    }
}
