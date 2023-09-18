using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using FLS.CodeBeispiel.FunctionApp.Logic;
using FLS.CodeBeispiel.FunctionApp.Models.External;

namespace FLS.CodeBeispiel.FunctionApp.Tests;

public class MeldungsFunctionAppTests
{

    public Meldung InitialeMeldung = new()
    {
        ExtPartner = "Partner Mustermann",
        ExtPartnerCrmId = Guid.Parse("6e338eec-a716-48a9-b6db-8983d08fc5c1"),
        Artikel = new[]
        {
            new Artikel
            {
                ExtArtikelnummer = "35346436",
                IntArtikelnummer = "254205",
                Bezeichnung = "Sprite",
                Gebinde = "24 X 0,33",
                Hersteller = "Coca-Cola Europacific Partners",
                FlaschenGTIN = null,
                GebindeGTIN = "5740600997625",
                TeilmengenGTIN = null
            },
            new Artikel
            {
                ExtArtikelnummer = "234628734",
                IntArtikelnummer = null,
                Bezeichnung = "Kellner Block",
                Gebinde = "1 X 1",
                Hersteller = "Holzwerke Hamburg",
                FlaschenGTIN = "239489327494789",
                GebindeGTIN = "5740600997625",
                TeilmengenGTIN = null
            }
        },
        Preisgruppe = new[]
        {
            new Preisgruppe
            {
                Bezeichnung = "Katalog 1",
                Nummer = 1,
            },
            new Preisgruppe
            {
                Bezeichnung = "Katalog 2",
                Nummer = 1,
            }
        }
    };

    public AutoMocker Mocker { get; }

    public MeldungsFunctionAppTests()
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
    }

    [Fact]
    public async Task EventsMitFalschemSubjectWerdenNichtVerarbeitet()
    {
        var sut = Mocker.CreateInstance<FunctionApp>();
        var @event = new EventGridEvent("Falsches Subject", "EventType", "1.0", InitialeMeldung);

        await sut.NeueMeldung(@event);

        Mocker.GetMock<ErgänzungsAntragsBearbeiter>().Verify(x => x.HandleErgänzungsAntrag(It.IsAny<Models.Domain.Meldung>()), Times.Never);
        Mocker.GetMock<ArtikelMeldungsBearbeiter>().Verify(x => x.HandleArtikelMeldung(It.IsAny<Models.Domain.Meldung>()), Times.Never);
    }

    [Fact]
    public async Task EventsMitKorrektemSubjectWerdenWeiterBearbeitet()
    {
        var sut = Mocker.CreateInstance<FunctionApp>();
        var @event = new EventGridEvent("DemoEventSubject", "EventType", "1.0", InitialeMeldung);

        await sut.NeueMeldung(@event);

        Mocker.GetMock<ErgänzungsAntragsBearbeiter>().Verify(x => x.HandleErgänzungsAntrag(It.Is<Models.Domain.Meldung>(sort => sort.Partner.Bezeichnung == InitialeMeldung.ExtPartner)), Times.Once);
        Mocker.GetMock<ArtikelMeldungsBearbeiter>().Verify(x => x.HandleArtikelMeldung(It.Is<Models.Domain.Meldung>(sort => sort.Partner.Bezeichnung == InitialeMeldung.ExtPartner)), Times.Once);
    }
}
