using Microsoft.Extensions.Logging;
using Moq;
using FLS.CodeBeispiel.CrmService.Models.CrmEntities;
using FLS.CodeBeispiel.CrmService.Models.Infrastructure;
using System.Net.Http.Json;
using System.Net;
using Moq.Protected;

namespace FLS.CodeBeispiel.CrmService.Tests;

public class KundenFürPartnerErmitteln
{
    private readonly Mock<ILogger<Infrastructure.CrmService>> _logger = new();
    private readonly Mock<HttpMessageHandler> _handler = new();
    private readonly HttpClient _httpClient = new();
    private Infrastructure.CrmService _crmService { get; set; }

    public KundenFürPartnerErmitteln()
    {
        _httpClient = new HttpClient(_handler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _crmService = new Infrastructure.CrmService(_httpClient, _logger.Object);
    }

    [Fact]
    public async Task FürEinenPartnerWerdenDieKundenKorrektErmittelt()
    {
        var accountGuid = Guid.NewGuid();

        var expectedResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new CrmBaseResponse<Account>()
            {
                Value = new Account[]
                {
                    new Account()
                    {
                        Accountid = Guid.NewGuid(),
                        Statuscode = 0,
                        _kgr_id = Guid.NewGuid(),
                    },
                    new Account()
                    {
                        Accountid = Guid.NewGuid(),
                        Statuscode = 0,
                        _kgr_id = Guid.NewGuid(),
                    },
                    new Account()
                    {
                        Accountid = Guid.NewGuid(),
                        Statuscode = 0,
                        _kgr_id = Guid.NewGuid(),
                    }
                }
            })
        };

        var expectedUri = new Uri($"http://localhost/api/data/v9.1/accs?$filter=(Hauptabrechner+eq+{accountGuid}+or+Nebenabrechner+eq+{accountGuid})+and+statuscode+eq+0&$select=accountid%2caccountnumber%2c_kgr_id&$expand=k_gr(%24select%3d_regionsArt_1%2c_regionsArt_2%2c_pgr_id)");

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedResponse));

        var result = await _crmService.GetAlleKundenFürPartner(accountGuid);

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task FürEinenPartnerOhneAktiveKundenWirdEineLeereListeZurückgegeben()
    {
        var accountGuid = Guid.NewGuid();

        var expectedResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new CrmBaseResponse<Account>()
            {
                Value = Array.Empty<Account>()
            })
        };

        var expectedUri = new Uri($"http://localhost/api/data/v9.1/accs?$filter=(Hauptabrechner+eq+{accountGuid}+or+Nebenabrechner+eq+{accountGuid})+and+statuscode+eq+0&$select=accountid%2caccountnumber%2c_kgr_id&$expand=k_gr(%24select%3d_regionsArt_1%2c_regionsArt_2%2c_pgr_id)");

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedResponse));

        var result = await _crmService.GetAlleKundenFürPartner(accountGuid);

        Assert.Empty(result);
    }

}
