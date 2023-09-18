using Microsoft.Extensions.DependencyInjection;
using FLS.CodeBeispiel.CrmService.Infrastructure;
using FLS.CodeBeispiel.CrmService.Models.Infrastructure;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;

namespace FLS.CodeBeispiel.CrmService.Extensions;

/// <summary>
/// Klasse mit Funktionalität, um den CrmService in der DI zu registrieren
/// </summary>
public static class CrmServiceExtension
{
    /// <summary>
    /// Funktion zur Registrierung des CrmService und des dazugehörigen typisierten HTTPClients mit RetryPolicy in der DI
    /// </summary>
    /// <param name="services">ServiceCollection, in der der CrmService registriert werden soll</param>
    /// <param name="crmConnectionSettings">Verbindungsinformationen zur Kommunikation mit dem CRM</param>
    /// <returns>Um den CrmService ergänzte ServiceCollection</returns>
    public static IServiceCollection AddCrmService(this IServiceCollection services, CrmConnectionSettings crmConnectionSettings)
    {
        services.AddTransient<ICrmService, Infrastructure.CrmService>();

        services.AddHttpClient<ICrmService, Infrastructure.CrmService>(c =>
        {
            c.Timeout = new TimeSpan(0, 2, 0);
            c.BaseAddress = crmConnectionSettings.URL;
            c.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            c.DefaultRequestHeaders.Add("OData-Version", "4.0");
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenProvider.GetToken(crmConnectionSettings).GetAwaiter().GetResult());
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    internal static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)));
    }
}
