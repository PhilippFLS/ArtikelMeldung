using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using FLS.CodeBeispiel.CrmService.Infrastructure;
using FLS.CodeBeispiel.KatalogService.Infrastructure;
using FLS.CodeBeispiel.FunctionApp.Abstractions;
using FLS.CodeBeispiel.FunctionApp.Logic;
using FLS.CodeBeispiel.FunctionApp.Models.Domain;

namespace FLS.CodeBeispiel.FunctionApp.Tests;

public class ErgänzungsAntragMailTest
{
    public const string KorrekterMailBody = "Partner 880062 <b>Partner Mustermann</b> hat die Aufnahme von Artikeln beantragt.<br><br>Artikel mit der MatNr 254205 <b>Sprite</b> 24 X 0,33 <br><br>Artikel <b>Kellner Block</b> 1 X 1 Holzwerke Hamburg<br>mit den EANs Gebinde: 5740600997625 Tray:  Flasche: 239489327494789<br>und der Partner-Artikelnummer 234628734<br><br>in die Kataloge<br><ul><li>Katalog 1</li>in die Regionen 1: Region1, Region3<br>in die Regionen 2: Region2, Region4<li>Katalog 2</li>in die Regionen 1: Region5<br>in die Regionen 2: Region6</ul><br>Viele Grüße aus der IT.";
    public const string KorrekterMailBetreff = "Neuer Ergänzungsantrag von Partner Mustermann";

    public Meldung InitialeMeldung = new()
    {
        Partner = new()
        {
            Bezeichnung = "Partner Mustermann",
            AccountId = Guid.Parse("6e338eec-a716-48a9-b6db-8983d08fc5c1"),
        },
        Artikel = new[]
        {
            new Artikel
            {
                ExtArtikelnummer = "35346436",
                IntNr = "254205",
                Bezeichnung = "Sprite",
                Gebinde = "24 X 0,33",
                Hersteller = "Coca-Cola Europacific Partners",
                FlaschenEAN = null,
                GebindeEAN = "5740600997625",
                TeilmengenEAN = null
            },
            new Artikel
            {
                ExtArtikelnummer = "234628734",
                IntNr = null,
                Bezeichnung = "Kellner Block",
                Gebinde = "1 X 1",
                Hersteller = "Holzwerke Hamburg",
                FlaschenEAN = "239489327494789",
                GebindeEAN = "5740600997625",
                TeilmengenEAN = null
            }
        },
        Katalog = new List<Katalog>
        {
            new Katalog
            {
                Bezeichnung = "Katalog 1",
                PreisGrNr = 1,
            },
            new Katalog
            {
                Bezeichnung = "Katalog 2",
                PreisGrNr = 2,
            }
        }
    };

    public ErgänzungsAntragsBearbeiter AntragsBearbeiter { get; set; }
    public IMailSender MailSender{ get; set; }
    public AutoMocker Mocker { get; }

    public ErgänzungsAntragMailTest()
    {
        Mocker = new AutoMocker();
        Mocker.Use(new Mock<BlobServiceClient>());
        Mocker.Use(new Mock<BlobContainerClient>());
        Mocker.Use(new Mock<BlobClient>());
        Mocker.Use(new Mock<ILoggerFactory>());

        Mocker.GetMock<IAzureClientFactory<BlobServiceClient>>()
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(Mocker.GetMock<BlobServiceClient>().Object);

        Mocker.GetMock<BlobServiceClient>()
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(Mocker.GetMock<BlobContainerClient>().Object);

        Mocker.GetMock<BlobContainerClient>()
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(Mocker.GetMock<BlobClient>().Object);

        Mocker.GetMock<ILoggerFactory>()
              .Setup(x => x.CreateLogger(It.IsAny<string>()))
              .Returns(Mocker.GetMock<ILogger>().Object);

        MailSender = Mocker.CreateInstance<MailSender>();
        AntragsBearbeiter = Mocker.CreateInstance<ErgänzungsAntragsBearbeiter>();
    }

    [Fact]
    public async Task AusEinerMeldungMitPreisgruppenWirdEineKorrekteMailAnsOrdersatzteamGeschickt()
    {
        var crmAccounts = new List<CrmService.Models.CrmEntities.Account>()
        {
            new()
            {
                Accountid = Guid.NewGuid(),
                _kgr_id = Guid.NewGuid(),
            },
            new()
            {
                Accountid = Guid.NewGuid(),
                _kgr_id = Guid.NewGuid(),
            },
            new()
            {
                Accountid = Guid.NewGuid(),
                _kgr_id = Guid.NewGuid(),
            }
        };

        var PreisgruppenPzos1 = new List<CrmService.Models.CrmEntities.KundenGruppe>()
        {
            new()
            {
                ID = (Guid)crmAccounts[0]._kgr_id!,
                _regionsArt_1 = Guid.NewGuid(),
                _regionsArt_2 = Guid.NewGuid(),
            },
            new()
            {
                ID = (Guid)crmAccounts[1]._kgr_id!,
                _regionsArt_1 = Guid.NewGuid(),
                _regionsArt_2 = Guid.NewGuid(),
            }
        };

        var PreisgruppenPzos2 = new List<CrmService.Models.CrmEntities.KundenGruppe>()
        {
            new()
            {
                ID = (Guid)crmAccounts[2]._kgr_id!,
                _regionsArt_1 = Guid.NewGuid(),
                _regionsArt_2 = Guid.NewGuid(),
            },
        };

        var regionen = new Dictionary<Guid, string>()
        {
            { Guid.NewGuid(), "Region1"},
            { Guid.NewGuid(), "Region2"},
            { Guid.NewGuid(), "Region3"},
            { Guid.NewGuid(), "Region4"},
            { Guid.NewGuid(), "Region5"},
            { Guid.NewGuid(), "Region6"},
        };

        Mocker.GetMock<ICrmService>()
                .Setup(x => x.GetNummerForAccount(It.Is<Guid>(y => y == InitialeMeldung.Partner.AccountId)))
                .Returns(Task.FromResult<int?>(880062));

        Mocker.GetMock<ICrmService>()
                .Setup(x => x.GetAlleKundenFürPartner(It.Is<Guid>(y => y == InitialeMeldung.Partner.AccountId)))
                .Returns(Task.FromResult(crmAccounts as IEnumerable<CrmService.Models.CrmEntities.Account>));

        Mocker.GetMock<ICrmService>()
                .Setup(x => x.GetAlleKundengruppenFürPreisgruppen(It.Is<int>(y => y == 1)))
                .Returns(Task.FromResult(PreisgruppenPzos1 as IEnumerable<CrmService.Models.CrmEntities.KundenGruppe>));

        Mocker.GetMock<ICrmService>()
                .Setup(x => x.GetAlleKundengruppenFürPreisgruppen(It.Is<int>(y => y == 2)))
                .Returns(Task.FromResult(PreisgruppenPzos2 as IEnumerable<CrmService.Models.CrmEntities.KundenGruppe>));

        Mocker.GetMock<IKatalogService>()
                .Setup(x => x.GetRegionFürKundeInEinerRegionsart(It.Is<Guid>(y => y == crmAccounts[0].Accountid), It.Is<Guid>(y => y == PreisgruppenPzos1[0]._regionsArt_1)))
                .Returns(Task.FromResult<Guid?>(regionen.First().Key));
        Mocker.GetMock<IKatalogService>()
                .Setup(x => x.GetRegionFürKundeInEinerRegionsart(It.Is<Guid>(y => y == crmAccounts[0].Accountid), It.Is<Guid>(y => y == PreisgruppenPzos1[0]._regionsArt_2)))
                .Returns(Task.FromResult<Guid?>(regionen.ElementAt(1).Key));

        Mocker.GetMock<IKatalogService>()
                .Setup(x => x.GetRegionFürKundeInEinerRegionsart(It.Is<Guid>(y => y == crmAccounts[1].Accountid), It.Is<Guid>(y => y == PreisgruppenPzos1[1]._regionsArt_1)))
                .Returns(Task.FromResult<Guid?>(regionen.ElementAt(2).Key));
        Mocker.GetMock<IKatalogService>()
                .Setup(x => x.GetRegionFürKundeInEinerRegionsart(It.Is<Guid>(y => y == crmAccounts[1].Accountid), It.Is<Guid>(y => y == PreisgruppenPzos1[1]._regionsArt_2)))
                .Returns(Task.FromResult<Guid?>(regionen.ElementAt(3).Key));

        Mocker.GetMock<IKatalogService>()
                .Setup(x => x.GetRegionFürKundeInEinerRegionsart(It.Is<Guid>(y => y == crmAccounts[2].Accountid), It.Is<Guid>(y => y == PreisgruppenPzos2[0]._regionsArt_1)))
                .Returns(Task.FromResult<Guid?>(regionen.ElementAt(4).Key));

        Mocker.GetMock<IKatalogService>()
                .Setup(x => x.GetRegionFürKundeInEinerRegionsart(It.Is<Guid>(y => y == crmAccounts[2].Accountid), It.Is<Guid>(y => y == PreisgruppenPzos2[0]._regionsArt_2)))
                .Returns(Task.FromResult<Guid?>(regionen.ElementAt(5).Key));

        foreach (var region in regionen)
        {
            Mocker.GetMock<IKatalogService>()
                    .Setup(x => x.GetRegionsBezeichnung(It.Is<Guid>(y => y == region.Key)))
                    .Returns(Task.FromResult(region.Value));
        }

        await AntragsBearbeiter.HandleErgänzungsAntrag(InitialeMeldung);
        
        Mocker.GetMock<IMailSender>().Verify(x => x.LadeMailInDenBlobStorageHoch(It.Is<IMailTemplate>(y => y.Subject == KorrekterMailBetreff && y.MailBody == KorrekterMailBody),It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EineMeldungOhnePreisgruppenIstKeinErgänzungsAntrag()
    {
        var meldungOhnePreisgruppen = new Meldung();
        await AntragsBearbeiter.HandleErgänzungsAntrag(meldungOhnePreisgruppen);

        Mocker.GetMock<IMailSender>().Verify(x => x.LadeMailInDenBlobStorageHoch(It.IsAny<IMailTemplate>(),It.IsAny<string>()),Times.Never);
    }
}