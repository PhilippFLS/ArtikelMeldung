namespace FLS.CodeBeispiel.FunctionApp.Models.Domain;

/// <summary>
/// Der GFGH, der die Meldung ausgelöst hat
/// </summary>
public class Partner
{
    /// <summary>
    /// Die Bezeichnung des auslösenden Gfghs
    /// </summary>
    public string Bezeichnung { get; init; } = string.Empty;

    /// <summary>
    /// Der AccountId des GFGHs, der die Meldung ausgelöst hat
    /// </summary>
    public Guid AccountId { get; init; }

    /// <summary>
    /// Der SAPNr des GFGHs, der die Meldung ausgelöst hat
    /// </summary>
    public int? Nummer { get; set; }
}
