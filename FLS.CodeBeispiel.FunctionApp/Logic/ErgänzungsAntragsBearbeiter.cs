using FLS.CodeBeispiel.CrmService.Infrastructure;
using FLS.CodeBeispiel.KatalogService.Infrastructure;
using FLS.CodeBeispiel.FunctionApp.Abstractions;
using FLS.CodeBeispiel.FunctionApp.Mapper;
using FLS.CodeBeispiel.FunctionApp.Models;
using FLS.CodeBeispiel.FunctionApp.Models.Domain;
using FLS.CodeBeispiel.FunctionApp.Models.MailTemplates;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FLS.CodeBeispiel.FunctionApp.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace FLS.CodeBeispiel.FunctionApp.Logic;

/// <summary>
/// Implementiert Geschäftslogik zu Sortimentsergänzungsanträge
/// </summary>
public class ErgänzungsAntragsBearbeiter
{
    private readonly ICrmService crmService;
    private readonly MailAdressen mailAdressen;
    private readonly IMailSender mailSender;
    private readonly IKatalogService ordersatzService;

    public ErgänzungsAntragsBearbeiter(ICrmService crmService, MailAdressen mailAdressen, IMailSender mailSender, IKatalogService ordersatzService)
    {
        this.crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
        this.mailAdressen = mailAdressen ?? throw new ArgumentNullException(nameof(mailAdressen));
        this.mailSender = mailSender ?? throw new ArgumentNullException(nameof(mailSender));
        this.ordersatzService = ordersatzService ?? throw new ArgumentNullException(nameof(ordersatzService));
    }

    /// <summary>
    /// Prüft, ob sich bei einer eingehenden Meldung um einen Ergänzungsantrag handelt.
    /// Wenn ja, wird eine Mail zur Benachrichtigung der Fachabteilung erstellt und diese
    /// auch verschickt.
    /// </summary>
    /// <param name="meldung">Die zu bearbeitende Meldung</param>
    public async virtual Task HandleErgänzungsAntrag(Meldung meldung)
    {
        if (HandeltEsSichBeiDerMeldungUmEinenErgänzungsAntrag(meldung))
        {
            var mail = await ErstelleAntragsMail(meldung);
            await VerschickeMail(mail);
        }
    }

    /// <summary>
    /// Prüfung, ob die Artikel aus der Meldung beantragt werden müssen. Dies geschieht durch Angabe von Katalogen, in die die Artikel eingetragen werden sollen.
    /// </summary>
    /// <param name="meldung">Die zu prüfende Meldung</param>
    /// <returns>Ob es sich um eine Meldung mit Artikeln handelt, die beantragt werden müssen</returns>
    private static bool HandeltEsSichBeiDerMeldungUmEinenErgänzungsAntrag(Meldung meldung) => meldung.Katalog.Any();

    private async Task<IMailTemplate> ErstelleAntragsMail(Meldung meldung)
    {
        meldung.Partner.Nummer = await crmService.GetNummerForAccount(meldung.Partner.AccountId);

        await ErgänzeKatalogeUmRegionen(meldung.Katalog, meldung.Partner.AccountId);

        return new AntragsMeldungsMail(meldung, mailAdressen.AntragsAdressen);
    }

    /// <summary>
    /// Erweitert für alle übergebenen Kataloge die Regionen, in denen der angegebene Partner Kunden zugeordnet hat
    /// </summary>
    /// <param name="kataloge">Alle Kataloge, die der Partner in der Meldung eingetragen hat</param>
    /// <param name="partnerAccountId">Die AccountId des Partners, der die Meldung ausgelöst hat</param>
    private async Task ErgänzeKatalogeUmRegionen(IEnumerable<Katalog> kataloge, Guid partnerAccountId)
    {
        var crmKundenDesPartner = await crmService.GetAlleKundenFürPartner(partnerAccountId);

        var preisgruppenDerKataloge = new List<Guid>();

        foreach (var strecke in kataloge)
        {
            var Preisgruppen = await crmService.GetAlleKGRsFürPgr(strecke.PreisGrNr);
            var domainPreisgruppen = Preisgruppen.Select(KgrMapper.MapToDomainKgr);
            strecke.Kgrs.AddRange(domainPreisgruppen);

            preisgruppenDerKataloge.AddRange(Preisgruppen.Select(p => p.ID));
        }

        var betroffeneKunden = crmKundenDesPartner
                                    .Select(AccountMapper.MapToDomainAccount)
                                    .Where(acc => acc.KgrId != Guid.Empty)
                                    .Where(acc => preisgruppenDerKataloge.Contains(acc.KgrId));

        foreach (var katalog in kataloge)
        {
            await ErmittleBetroffeneRegionenFürKatalog(katalog, betroffeneKunden);
        }
    }


    /// <summary>
    /// Sucht für alle übergebenen Accounts ihre Regionen entsprechend ihrer zugeordneten Kundengruppe mit deren Regionsart hinaus
    /// und schreibt diese Information an die entsprechende Kundengruppe des Kataloges
    /// </summary>
    /// <param name="katalog">Der Katalog für den die Regionen ergänzt werden sollen</param>
    /// <param name="accounts">Alle Accounts deren Regionen eingetragen werden sollen</param>
    /// <returns>Der um Regionen 1 und 2 ergänzte Katalog</returns>
    private async Task ErmittleBetroffeneRegionenFürKatalog(Katalog katalog, IEnumerable<Account> accounts)
    {
        if (!katalog.Kgrs.Any())
        {
            return;
        }

        foreach (var account in accounts)
        {
            await ErmittleRegion1(katalog.Kgrs, account);
            await ErmittleRegion2(katalog.Kgrs, account);
        }
    }

    /// <summary>
    /// Fügt die Region 2 eines Kunden zu der ihm zugeordneten Kundengruppe hinzu, sofern diese dort noch nicht vorhanden ist
    /// </summary>
    /// <param name="kundenGruppen">Liste der Kundengruppen, die einem Katalog zugeordnet sind</param>
    /// <param name="account">Der Account des Kunden</param>
    private async Task ErmittleRegion2(IEnumerable<Kgr> kundenGruppen, Account account)
    {
        var regionsArt_2 = kundenGruppen.Where(kgr => kgr.KgrId == account.KgrId)
                               .Where(kgr => kgr.RegionsArt_2 is not null)
                               .Select(kgr => kgr.RegionsArt_2)
                               .SingleOrDefault();

        if (regionsArt_2 is null)
        {
            return;
        }

        var region_2 = await ordersatzService.GetRegionFürKundeInEinerRegionsart(account.CrmAccountId, (Guid)regionsArt_2!);

        if (region_2 is null)
        {
            return;
        }

        var bezeichnung = await ordersatzService.GetRegionsBezeichnung((Guid)region_2);
        var vorhandeneRegionen_2 = kundenGruppen.Single(kgr => kgr.KgrId == account.KgrId).Regionen_2.ToList();

        if (!vorhandeneRegionen_2.Contains(bezeichnung))
        {
            kundenGruppen.Single(kgr => kgr.KgrId == account.KgrId).Regionen_2.Add(bezeichnung);
        }
    }

    /// <summary>
    /// Fügt die Region 1 eines Kunden zu der ihm zugeordneten Kundengruppe hinzu, sofern diese dort noch nicht vorhanden ist
    /// </summary>
    /// <param name="kundenGruppen">Liste der Kundengruppen, die einem Katalog zugeordnet sind</param>
    /// <param name="account">Der Account des Kunden</param>
    private async Task ErmittleRegion1(IEnumerable<Kgr> kundenGruppen, Account account)
    {
        var regionsArt_1 = kundenGruppen.Where(kgr => kgr.KgrId == account.KgrId)
                               .Where(kgr => kgr.RegionsArt_1 is not null)
                               .Select(kgr => kgr.RegionsArt_1)
                               .SingleOrDefault();

        if (regionsArt_1 is null)
        {
            return;
        }

        var region_1 = await ordersatzService.GetRegionFürKundeInEinerRegionsart(account.CrmAccountId,(Guid)regionsArt_1!);

        if (region_1 is null)
        {
            return;
        }

        var bezeichnung = await ordersatzService.GetRegionsBezeichnung((Guid)region_1);
        var vorhandeneRegionen_1 = kundenGruppen.Single(kgr => kgr.KgrId == account.KgrId).Regionen_1.ToList();

        if (!vorhandeneRegionen_1.Contains(bezeichnung))
        {
            kundenGruppen.Single(kgr => kgr.KgrId == account.KgrId).Regionen_1.Add(bezeichnung);
        }
    }

    /// <summary>
    /// Hiermit wird die erzeugte Mail verschickt
    /// Achtung: Im momentanen Prozess gibt es keine Möglichkeit unsererseits, Mails direkt aus einer FunctionApp zu versenden!
    /// Der Workaround ist hier, eine Datei in einem BlobStorage abzulegen, woraufhin ein anderer Prozess diese weiterverschickt.
    /// </summary>
    /// <param name="mail">Die zu verschickende Mail</param>
    private async Task VerschickeMail(IMailTemplate mail)
    {
        var dateiName = $"ErgaenzungsAntrag_{DateTime.Now:dd'_'MM'_'yyyy'T'HH'_'mm'_'ss}.json";

        await mailSender.LadeMailInDenBlobStorageHoch(mail, dateiName);
    }
}
