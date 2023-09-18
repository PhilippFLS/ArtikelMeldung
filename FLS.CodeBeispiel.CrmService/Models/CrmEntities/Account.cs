namespace FLS.CodeBeispiel.CrmService.Models.CrmEntities;

#pragma warning disable IDE1006 // Benennungsstile - diese Namen wurden automatisch im CRM generiert
public record Account
{
    public Guid Accountid { get; set; }

    public string? Accountnumber { get; set; }

    public Guid? Hauptabrechner { get; set; }

    public Guid? Nebenabrechner { get; set; }

    public Guid? _kgr_id { get; set; }

    public int Statuscode { get; set; }

    public KundenGr K_Gr { get; set; } = new();
}
#pragma warning restore IDE1006 // Benennungsstile
