namespace FLS.CodeBeispiel.FunctionApp.Models.Domain;

public class Kgr
{
    public Guid KgrId { get; set; }

    public Guid? RegionsArt_1 { get; set; }

    public Guid? RegionsArt_2 { get; set; }

    /// <summary>
    /// Die Bezeichnungen der Regionen nach Typ 1, die diese Kgr aufweist
    /// Achtung: Dies sind nicht alle Regionen, die es für die Kgr generell gibt, sondern die,
    /// in denen der Partner auch Kunden für die Kgr zugeordnet ist
    /// </summary>
    public List<string> Regionen_1 { get; set; } = new List<string>();

    /// <summary>
    /// Die Bezeichnungen der Regionen nach Typ 2, die diese Kgr aufweist
    /// Achtung: Dies sind nicht alle Regionen, die es für die Kgr generell gibt, sondern die,
    /// in denen der Partner auch Kunden für die Kgr zugeordnet ist
    /// </summary>
    public List<string> Regionen_2 { get; set; } = new List<string>();
}
