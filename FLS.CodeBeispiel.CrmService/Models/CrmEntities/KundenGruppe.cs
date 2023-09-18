namespace FLS.CodeBeispiel.CrmService.Models.CrmEntities;

#pragma warning disable IDE1006 // Benennungsstile - diese Namen wurden automatisch im CRM generiert

/// <summary>
/// Zusammenfassung von verschiedenen Kunden mit gemeinsamen Eigenschaften
/// </summary>
public record KundenGruppe
{
    public Guid ID { get; set; }

    /// <summary>
    /// Alternative ID der Kundengruppe
    /// </summary>
    public Guid _ag_kgrid_value { get; set; }

    /// <summary>
    /// ID der Preisgruppe, der die Kundengruppe zugeordnet ist
    /// </summary>
    public Guid? _pgr_id { get; set; }

    /// <summary>
    /// Regionsart vom Typ 1
    /// </summary>
    public Guid? _regionsArt_1 { get; set; }

    /// <summary>
    /// Regionsart vom Typ 2
    /// </summary>
    public Guid? _regionsArt_2 { get; set; }
}
#pragma warning restore IDE1006 // Benennungsstile
