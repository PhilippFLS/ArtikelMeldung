namespace FLS.CodeBeispiel.CrmService.Models.CrmEntities;

/// <summary>
/// Preisgruppe im CRM - gleichbedeutend mit einem Katalog
/// </summary>
public record PreisGruppe
{
    public string Bezeichnung { get; set; } = string.Empty;

    public Guid? Id { get; set; }

    public int? Nummer { get; set; }
}
