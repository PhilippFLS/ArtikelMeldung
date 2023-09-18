using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using FLS.CodeBeispiel.CrmService.Infrastructure;
using FLS.CodeBeispiel.FunctionApp.Abstractions;
using FLS.CodeBeispiel.FunctionApp.Logic;
using FLS.CodeBeispiel.FunctionApp.Models.Domain;

namespace FLS.CodeBeispiel.FunctionApp.Tests;

public class ArtikelMeldungsMailTest
{
    public const string KorrekterMailBody = "Partner 880062 <b>Partner Mustermann</b> hat seine Artikelnummer(n) mitgeteilt:<br><br><ul><li>Artikel <b>254244</b> Fanta Strawberry Kiwi, 24 X 0,33 hat die Nummer 8732478365.</li></ul>Viele Grüße aus der IT.";

    public const string KorrekterMailBetreff = "Neue Artikelnummer(n) von Partner Mustermann";

    public Meldung InitialeMeldung = new()
    {
        Partner = new Partner()
        {
            Bezeichnung = "Partner Mustermann",
            AccountId = Guid.Parse("6e338eec-a716-48a9-b6db-8983d08fc5c1")
        },
        Artikel = new[]
    {
            new Artikel
            {
                ExtArtikelnummer = "8732478365",
                IntNr = "254244",
                Bezeichnung = "Fanta Strawberry Kiwi",
                Gebinde = "24 X 0,33",
                Hersteller = "Coca-Cola Europacific Partners",
                FlaschenEAN = null,
                GebindeEAN = "5740700987977",
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
                Bezeichnung = "Strecke EK Basis",
                PreisGrNr = 1,
            }
        }
    };

    public ArtikelMeldungsBearbeiter ArtikelMeldungsBearbeiter { get; set; }
    public IMailSender MailSender { get; set; }
    public AutoMocker Mocker { get; }

    public ArtikelMeldungsMailTest()
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
        ArtikelMeldungsBearbeiter = Mocker.CreateInstance<ArtikelMeldungsBearbeiter>();
    }

    [Fact]
    public async Task AusEinerMeldungMitBekanntenArtikelnWirdEineKorrekteMailGeschickt()
    {
        Mocker.GetMock<ICrmService>()
                .Setup(x => x.GetNummerForAccount(It.Is<Guid>(y => y == InitialeMeldung.Partner.AccountId)))
                .Returns(Task.FromResult<int?>(880062));

        await ArtikelMeldungsBearbeiter.HandleArtikelMeldung(InitialeMeldung);

        Mocker.GetMock<IMailSender>().Verify(x => x.LadeMailInDenBlobStorageHoch(It.Is<IMailTemplate>(y => y.Subject == KorrekterMailBetreff && y.MailBody == KorrekterMailBody), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EineMeldungOhneBekannteArtikelIstKeineArtikelMeldung()
    {
        var meldungOhneBekannteArtikel = new Meldung()
        {
            Artikel = InitialeMeldung.Artikel.Where(art => art.IntNr is null)
        };
        await ArtikelMeldungsBearbeiter.HandleArtikelMeldung(meldungOhneBekannteArtikel);

        Mocker.GetMock<IMailSender>().Verify(x => x.LadeMailInDenBlobStorageHoch(It.IsAny<IMailTemplate>(), It.IsAny<string>()), Times.Never);
    }
}
