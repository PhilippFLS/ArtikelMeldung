using FLS.CodeBeispiel.FunctionApp.Abstractions;
using FLS.CodeBeispiel.FunctionApp.Models.Domain;
using System.Text;

namespace FLS.CodeBeispiel.FunctionApp.Models.MailTemplates;

public class ArtikelMeldungsMail : IMailTemplate
{
    public IEnumerable<string> Recipients { get; set; }
    public IEnumerable<string> CCRecipients { get; set; } = Array.Empty<string>();
    public string Subject { get; set; }
    public string MailBody { get; set; }

    public ArtikelMeldungsMail(Meldung meldung, IEnumerable<string> empfängerAdressen)
    {
        Recipients = empfängerAdressen;
        Subject = CreateMailSubject(meldung.Partner.Bezeichnung);
        MailBody = CreateMailBody(meldung);
    }

    internal static string CreateMailSubject(string beantragenderPartnerName) => $"Neue Artikelnummer(n) von {beantragenderPartnerName}";

    internal static string CreateMailBody(Meldung meldung)
    {
        var sb = new StringBuilder();
        sb.Append($"Partner {meldung.Partner.Nummer} <b>{meldung.Partner.Bezeichnung}</b> hat seine Artikelnummer(n) mitgeteilt:");
        sb.Append("<br>");
        sb.Append("<br>");

        sb.Append("<ul>");

        foreach (var artikel in meldung.Artikel.Where(art => art.IntNr is not null))
        {
            sb.Append($"<li>Artikel <b>{artikel.IntNr}</b> {artikel.Bezeichnung}, {artikel.Gebinde} hat die Nummer {artikel.ExtArtikelnummer}.</li>");
        }

        sb.Append("</ul>");

        sb.Append("Viele Grüße aus der IT.");

        return sb.ToString();
    }
}
