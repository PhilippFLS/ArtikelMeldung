using FLS.CodeBeispiel.CrmService.Extensions;
using FLS.CodeBeispiel.CrmService.Models.Infrastructure;
using FLS.CodeBeispiel.FunctionApp;
using FLS.CodeBeispiel.FunctionApp.Abstractions;
using FLS.CodeBeispiel.FunctionApp.Logic;
using FLS.CodeBeispiel.FunctionApp.Models;
using FLS.CodeBeispiel.KatalogService;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        var config = s.BuildServiceProvider().GetService<IConfiguration>();
        var crmSettings = config?.GetSection(Constants.CrmServiceConnection).Get<CrmConnectionSettings>() ?? throw new ArgumentException("Die Verbindungsinformationen für das CRM konnten nicht geladen werden");
        var mailAdressen = new MailAdressen();
        config.GetSection(nameof(MailAdressen)).Bind(mailAdressen);

        s.AddCrmService(crmSettings);
        s.AddScoped(services => mailAdressen);
        s.AddScoped<ErgänzungsAntragsBearbeiter>();
        s.AddScoped<ArtikelMeldungsBearbeiter>();
        s.AddScoped<IMailSender, MailSender>();
        s.AddKatalogService(config[Constants.KatalogConnectionString] ?? throw new ArgumentException("Die Verbindungsinformationen für die KatalogDB konnten nicht geladen werden"));
        s.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(config[Constants.StorageAccountConnection]);
        });
    })
    .Build();

host.Run();