namespace FLS.CodeBeispiel.FunctionApp.Models;

public class MailAdressen
{
    public IEnumerable<string> AntragsAdressen { get; set; } = Array.Empty<string>();

    public IEnumerable<string> ArtikelMeldungsAdressen { get; set; } = Array.Empty<string>();
}
