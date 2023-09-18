namespace FLS.CodeBeispiel.FunctionApp.Models.Domain;

public record Katalog
{
    public string Bezeichnung { get; init; } = string.Empty;

    public int PreisGrNr { get; init; }

    public List<Kgr> Kgrs { get; set; } = new List<Kgr>();
}
