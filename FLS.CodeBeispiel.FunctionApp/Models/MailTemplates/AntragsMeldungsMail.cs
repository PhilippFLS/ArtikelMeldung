using FLS.CodeBeispiel.FunctionApp.Abstractions;
using FLS.CodeBeispiel.FunctionApp.Models.Domain;
using System.Text;

namespace FLS.CodeBeispiel.FunctionApp.Models.MailTemplates;

public class AntragsMeldungsMail : IMailTemplate
{
    public IEnumerable<string> Recipients { get; set; }
    public IEnumerable<string> CCRecipients { get; set; } = Array.Empty<string>();
    public string Subject { get; set; }
    public string MailBody { get; set; }

    public AntragsMeldungsMail(Meldung meldung, IEnumerable<string> empfängerAdressen)
    {
        Recipients = empfängerAdressen;
        Subject = CreateMailSubject(meldung.Partner.Bezeichnung);
        MailBody = CreateMailBody(meldung);
    }

    internal static string CreateMailSubject(string beantragenderPartnerName) => $"Neuer Ergänzungsantrag von {beantragenderPartnerName}";

    internal static string CreateMailBody(Meldung meldung)
    {
        var sb = new StringBuilder();
        sb.Append($"Partner {meldung.Partner.Nummer} <b>{meldung.Partner.Bezeichnung}</b> hat die Aufnahme von Artikeln beantragt.");
        sb.Append("<br>");
        sb.Append("<br>");

        foreach (var artikel in meldung.Artikel)
        {
            CreateArtikelRow(artikel, sb);
            sb.Append("<br>");
        }

        CreateKatalogRows(meldung.Katalog, sb);
        sb.Append("<br>");

        sb.Append("Viele Grüße aus der IT.");

        return sb.ToString();
    }

    internal static string CreateArtikelRow(Artikel artikel, StringBuilder builder)
    {
        if (artikel.IntNr != null)
        {
            builder.Append($"Artikel mit der MatNr {artikel.IntNr} <b>{artikel.Bezeichnung}</b> {artikel.Gebinde} ");
            builder.Append("<br>");
        }
        else
        {
            builder.Append($"Artikel <b>{artikel.Bezeichnung}</b> {artikel.Gebinde} {artikel.Hersteller}");
            builder.Append("<br>");
            builder.Append($"mit den EANs Gebinde: {artikel.GebindeEAN} Tray: {artikel.TeilmengenEAN} Flasche: {artikel.FlaschenEAN}");
            builder.Append("<br>");
            builder.Append($"und der Partner-Artikelnummer {artikel.ExtArtikelnummer}");
            builder.Append("<br>");
        }

        return builder.ToString();
    }

    internal static string CreateKatalogRows(IEnumerable<Katalog> kataloge, StringBuilder builder)
    {
        builder.Append("in die Kataloge");
        builder.Append("<br>");
        builder.Append("<ul>");

        foreach (var katalog in kataloge)
        {
            var regionen1 = katalog.Kgrs.SelectMany(pzo => pzo.Regionen_1).Distinct();
            var regionen2 = katalog.Kgrs.SelectMany(pzo => pzo.Regionen_2).Distinct();

            builder.Append($"<li>{katalog.Bezeichnung}</li>");
            builder.Append($"in die Regionen 1: {string.Join(", ", regionen1)}");
            builder.Append("<br>");
            builder.Append($"in die Regionen 2: {string.Join(", ", regionen2)}");
        }

        builder.Append("</ul>");

        return builder.ToString();
    }
}
