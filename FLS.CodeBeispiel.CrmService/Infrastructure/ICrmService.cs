using FLS.CodeBeispiel.CrmService.Models.CrmEntities;

namespace FLS.CodeBeispiel.CrmService.Infrastructure;

/// <summary>
/// Interface für Abfragen an das CRM/>
/// </summary>
public interface ICrmService
{
    /// <summary>
    /// Funktion zum Ermitteln der Nummer eines bestehenden Accounts im CRM
    /// </summary>
    /// <param name="accountId">Die ID des Accounts, für den die Nummer ermittelt werden soll</param>
    /// <returns>Die Nummer des Accounts, sofern vorhanden</returns>
    Task<int?>GetNummerForAccount(Guid accountId);

    /// <summary>
    /// Funktion zum Ermitteln aller Kunden eines Partners als Haupt- oder Nebenabrechner
    /// </summary>
    /// <param name="partnerAccountId">Die AccountID des Partners im CRM</param>
    /// <returns>Liste von dem Partner zugeordneten Kunden</returns>
    Task<IEnumerable<Account>> GetAlleKundenFürPartner(Guid partnerAccountId);

    /// <summary>
    /// Funktion zum Ermitteln aller KundengruppenIds, die die angegebene Preisgruppe zugeordnet haben
    /// </summary>
    /// <param name="pgrNummer">Die Nummer der Preisgruppe</param>
    /// <returns>Liste der Kundengruppen</returns>
    Task<IEnumerable<KundenGruppe>> GetAlleKundengruppenFürPreisgruppen(int pgrNummer);
}