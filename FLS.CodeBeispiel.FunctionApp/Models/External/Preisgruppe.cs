namespace FLS.CodeBeispiel.FunctionApp.Models.External;

public record Preisgruppe
{
    public string Bezeichnung { get; init; } = string.Empty;

    public int Nummer { get; init; }
}
