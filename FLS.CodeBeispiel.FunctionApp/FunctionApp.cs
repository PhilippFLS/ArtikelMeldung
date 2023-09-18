// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FLS.CodeBeispiel.FunctionApp.Logic;
using FLS.CodeBeispiel.FunctionApp.Mapper;
using FLS.CodeBeispiel.FunctionApp.Models.External;
using System.Text.Json;

namespace FLS.CodeBeispiel.FunctionApp;

public class FunctionApp
{
    private readonly ILogger _logger;
    private readonly string EventSubject = "DemoEventSubject";
    private readonly ErgänzungsAntragsBearbeiter antragsBearbeiter;
    private readonly ArtikelMeldungsBearbeiter artikelMeldungsBearbeiter;

    public FunctionApp(ILoggerFactory loggerFactory, ErgänzungsAntragsBearbeiter antragsBearbeiter, ArtikelMeldungsBearbeiter artikelNrMeldungsBearbeiter)
    {
        _logger = loggerFactory.CreateLogger<FunctionApp>();
        this.antragsBearbeiter = antragsBearbeiter;
        this.artikelMeldungsBearbeiter = artikelNrMeldungsBearbeiter;
    }

    [Function(nameof(NeueMeldung))]
    public async Task NeueMeldung([EventGridTrigger] EventGridEvent input)
    {
        if (input.Subject != EventSubject)
        {
            return;
        }

        var externalEvent = input.Data.ToObjectFromJson<Meldung>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        _logger.LogInformation($"Neue Meldung empfangen für {externalEvent.ExtPartner}");

        var domainSortimentsMeldung = externalEvent.MapToDomainMeldung();

        await antragsBearbeiter.HandleErgänzungsAntrag(domainSortimentsMeldung);
        await artikelMeldungsBearbeiter.HandleArtikelMeldung(domainSortimentsMeldung);
    }
}
