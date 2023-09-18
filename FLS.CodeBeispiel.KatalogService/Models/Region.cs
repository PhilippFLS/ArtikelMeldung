namespace FLS.CodeBeispiel.KatalogService.Models;

public record Region
{
    public int Nummer { get; set; }
    public int RegionsartID { get; set; }
    public string Bezeichnung { get; set; } = string.Empty;
    public Guid? RegionsID { get; set; }
}
