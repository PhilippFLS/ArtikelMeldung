using Microsoft.Extensions.Logging;
using FLS.CodeBeispiel.CrmService.Enumerations;
using FLS.CodeBeispiel.CrmService.Extensions;
using FLS.CodeBeispiel.CrmService.Models.CrmEntities;
using FLS.CodeBeispiel.CrmService.Models.Infrastructure;
using System.Runtime.CompilerServices;
using System.Text.Json;

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
    public async Task<IEnumerable<Account>> GetAlleKundenFürPartner(Guid gfghAccountId)
    {
        var customFilter = new ODataFilter<string>()
        {
            Key = $"({nameof(Account.Hauptabrechner)} {ODataOperators.eq} {gfghAccountId} or {nameof(Account.Nebenabrechner)} {ODataOperators.eq} {gfghAccountId})",
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
                                      .WithExpand(nameof(Account.K_Gr).ToLower(), new string[] { nameof(KundenGr._regionsArt_1), nameof(KundenGr._regionsArt_2), nameof(KundenGr._pgr_id) })
                                      .BuildUri();

        var zugeordneteAccounts = await GetHttpResponseAsync<Account>(zugeordneteAccountsUri);

        return zugeordneteAccounts?.Value ?? Array.Empty<Account>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<KundenGr>> GetAlleKGRsFürPgr(int preisgruppenNummer)
    {
        var PgrnGuid = await GetPgrGuid(preisgruppenNummer);

        if (PgrnGuid is null)
        {
            return Array.Empty<KundenGr>();
        }

        return await GetZugeordneteKgrFürPgr((Guid)PgrnGuid);
    }

    /// <summary>
    /// Ermittelt für eine Pgrnnummer die ID der Pgr im CRM
    /// </summary>
    /// <param name="PgrnNummer">Die zugeordnete Preisgruppennummer einer Pgr</param>
    /// <returns>Die ID der Pgr</returns>
    private async Task<Guid?> GetPgrGuid(int PgrnNummer)
    {
        var PgrnFilter = new ODataFilter<int?>()
        {
            Key = nameof(PreisGr.Nummer).ToLower(),
            Value = PgrnNummer
        };

        var PgrnUri = new UriBuilder(httpClient.BaseAddress!.ToString())
                                .SetBaseEntity(CrmTableNames.PreisGr)
                                .WithFilter(PgrnFilter)
                                .WithSelect(nameof(PreisGr.Id).ToLower())
                                .BuildUri();

        var Pgr = await GetHttpResponseAsync<PreisGr>(PgrnUri);

        return Pgr != null && Pgr.Value.Any() ? Pgr.Value[0].Id : null;
    }

    /// <summary>
    /// Ermittelt für eine angegebene Pgr alle KGRs, die die Pgr gesetzt haben
    /// </summary>
    /// <param name="PgrnGuid">Die CRM-ID der Pgr</param>
    /// <returns>Liste von allen KGRs, die der Pgr zugeordnet sind</returns>
    private async Task<IEnumerable<KundenGr>> GetZugeordneteKgrFürPgr(Guid PgrnGuid)
    {
        var kgrFilter = new ODataFilter<Guid>()
        {
            Key = nameof(KundenGr._pgr_id),
            Value = PgrnGuid
        };

        var kgrUri = new UriBuilder(httpClient.BaseAddress!.ToString())
                                .SetBaseEntity(CrmTableNames.KundenGr)
                                .WithFilter(kgrFilter)
                                .WithSelect(nameof(KundenGr.ID).ToLower())
                                .WithSelect(nameof(KundenGr._regionsArt_1))
                                .WithSelect(nameof(KundenGr._regionsArt_2))
                                .BuildUri();

        var kgrs = await GetHttpResponseAsync<KundenGr>(kgrUri);

        return kgrs?.Value ?? Array.Empty<KundenGr>();
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
