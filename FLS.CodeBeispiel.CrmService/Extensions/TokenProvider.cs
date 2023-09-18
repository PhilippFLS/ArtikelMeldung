using Microsoft.Identity.Client;
using FLS.CodeBeispiel.CrmService.Models.Infrastructure;

namespace FLS.CodeBeispiel.CrmService.Extensions;

/// <summary>
/// Hilfsklasse zur Tokengenerierung
/// </summary>
internal static class TokenProvider
{
    /// <summary>
    /// Funktion zur Generierung eines AccessTokens für das CRM
    /// </summary>
    /// <param name="settings">Verbindungsinformationen zur Kommunikation mit dem CRM</param>
    /// <returns>AccessToken zur Authentifizierung am CRM</returns>
    public static async Task<string> GetToken(CrmConnectionSettings settings)
    {
        var client = ConfidentialClientApplicationBuilder
                    .Create(settings.ClientID)
                    .WithAuthority(new Uri($"https://token-url.com/{settings.TenantID}/oauth2/token"))
                    .WithClientSecret(settings.ClientSecret)
                    .Build();

        var scopes = new string[] { settings.Scope };
        var authResult = await client.AcquireTokenForClient(scopes).ExecuteAsync();
        return authResult.AccessToken;
    }
}
