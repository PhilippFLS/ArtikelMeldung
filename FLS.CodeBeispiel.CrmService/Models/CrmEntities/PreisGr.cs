namespace FLS.CodeBeispiel.CrmService.Models.CrmEntities;

/// <summary>
/// Preisgruppe im CRM - gleichbedeutend mit einer Strecke/ einem Ordersatz
/// </summary>
public record PreisGr
{
    public string Bezeichnung { get; set; } = string.Empty;

    public Guid? Id { get; set; }

    public int? Nummer { get; set; }
}
