using FLS.CodeBeispiel.CrmService.Infrastructure;
using FLS.CodeBeispiel.FunctionApp.Abstractions;
using FLS.CodeBeispiel.FunctionApp.Models;
using FLS.CodeBeispiel.FunctionApp.Models.Domain;
using FLS.CodeBeispiel.FunctionApp.Models.MailTemplates;

namespace FLS.CodeBeispiel.FunctionApp.Logic;

/// <summary>
/// Beinhaltet Geschäftslogik für die Meldung von GFGH-Artikelnummern
/// </summary>
public class ArtikelMeldungsBearbeiter
{
    private readonly ICrmService crmService;
    private readonly MailAdressen mailAdressen;
    private readonly IMailSender mailSender;

    public ArtikelMeldungsBearbeiter(ICrmService crmService, MailAdressen mailAdressen, IMailSender mailSender)
    {
        this.crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
        this.mailAdressen = mailAdressen ?? throw new ArgumentNullException(nameof(mailAdressen));
        this.mailSender = mailSender ?? throw new ArgumentNullException(nameof(mailSender));
    }

    /// <summary>
    /// Prüft und verarbeitet eine eingegangene Artikelmeldung.
    /// Verschickt im Erfolgsfall eine Mail an die Fachabteilung.
    /// </summary>
    /// <param name="meldung">Die eingegangene Meldung</param>
    public async virtual Task HandleArtikelMeldung(Meldung meldung)
    {
        if (MüssenArtikelGemeldetWerden(meldung.Artikel))
        {
            var mail = await ErstelleArtikelMeldungsMailFürAbrechnung(meldung);
            await VerschickeMail(mail);
        }
    }

    /// <summary>
    /// Prüfung, ob in der Meldung externe Artikelnummern vorhanden sind, die gemeldet werden müssen
    /// </summary>
    /// <param name="artikel">Beantragte Artikel aus einer Meldung</param>
    /// <returns>Ob in der Meldung für bekannte Artikel externe ArtikelNr gemeldet wurden</returns>
    private static bool MüssenArtikelGemeldetWerden(IEnumerable<Artikel> artikel) => artikel.Any(art => art.IntNr is not null);

    /// <summary>
    /// Bereitet die Informationen aus dem Antrag in Textform auf.
    /// </summary>
    /// <param name="meldung">Die Meldung, aus der die Mail erstellt werden soll</param>
    /// <returns>Eine zu verschickende Mail inklusive HTML-Tags</returns>
    private async Task<IMailTemplate> ErstelleArtikelMeldungsMailFürAbrechnung(Meldung meldung)
    {
        meldung.Partner.Nummer = await crmService.GetNummerForAccount(meldung.Partner.AccountId);

        return new ArtikelMeldungsMail(meldung, mailAdressen.ArtikelMeldungsAdressen);
    }

    /// <summary>
    /// Hiermit wird die erzeugte Mail verschickt
    /// Achtung: Im momentanen Prozess gibt es keine Möglichkeit unsererseits, Mails direkt aus einer FunctionApp zu versenden!
    /// Der Workaround ist hier, eine Datei in einem BlobStorage abzulegen, woraufhin ein anderer Prozess diese weiterverschickt.
    /// </summary>
    /// <param name="mail">Die zu verschickende Mail</param>
    private async Task VerschickeMail(IMailTemplate mail)
    {
        var dateiName = $"ArtikelNrMeldung_{DateTime.Now:dd'_'MM'_'yyyy'T'HH'_'mm'_'ss}.json";

        await mailSender.LadeMailInDenBlobStorageHoch(mail, dateiName);
    }
}
