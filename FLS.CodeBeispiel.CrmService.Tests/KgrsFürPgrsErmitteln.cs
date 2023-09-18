using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using FLS.CodeBeispiel.CrmService.Models.CrmEntities;
using FLS.CodeBeispiel.CrmService.Models.Infrastructure;
using System.Net.Http.Json;
using System.Net;

namespace FLS.CodeBeispiel.CrmService.Tests;

public class KgrsFürPgrsErmitteln
{
    private readonly Mock<ILogger<Infrastructure.CrmService>> _logger = new();
    private readonly Mock<HttpMessageHandler> _handler = new();
    private readonly HttpClient _httpClient = new();
    private Infrastructure.CrmService _crmService { get; set; }

    public KgrsFürPgrsErmitteln()
    {
        _httpClient = new HttpClient(_handler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _crmService = new Infrastructure.CrmService(_httpClient, _logger.Object);
    }

    [Fact]
    public async Task FürEinePgrWerdenDieKgrsKorrektErmittelt()
    {
        var preisgruppenNummer = 123;
        var pgrGuid = Guid.NewGuid();

        var expectedPgrGuidResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new CrmBaseResponse<PreisGr>()
            {
                Value = new PreisGr[]
                {
                    new PreisGr()
                    {
                        Id = pgrGuid
                    }
                }
            })
        };

        var expectedKgrResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new CrmBaseResponse<KundenGr>()
            {
                Value = new KundenGr[]
                {
                    new KundenGr()
                    {
                        _pgr_id = pgrGuid,
                        _ag_kgrid_value = Guid.NewGuid(),
                        _regionsArt_1 = Guid.NewGuid(),
                        _regionsArt_2 = null,
                    },
                     new KundenGr()
                    {
                        _pgr_id = pgrGuid,
                        _ag_kgrid_value = Guid.NewGuid(),
                        _regionsArt_1 = Guid.NewGuid(),
                        _regionsArt_2 = Guid.NewGuid(),
                    }
                }
            })
        };

        var expectedStreckenGuidUri = new Uri($"http://localhost/api/data/v9.1/pgrs?$filter=nummer+eq+123&$select=id");

        var expectedKgrUri = new Uri($"http://localhost/api/data/v9.1/kgrs?$filter=_pgr_id+eq+%27{pgrGuid}%27&$select=id%2c_regionsArt_1%2c_regionsArt_2");

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedStreckenGuidUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedPgrGuidResponse));

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedKgrUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedKgrResponse));

        var result = await _crmService.GetAlleKGRsFürPgr(preisgruppenNummer);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FürEinePgrOhneKgrsWirdEineLeereListeZurückgegeben()
    {
        var pgrNummer = 123;
        var pgrGuid = Guid.NewGuid();

        var expectedPgrGuidResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new CrmBaseResponse<PreisGr>()
            {
                Value = new PreisGr[]
                {
                    new PreisGr()
                    {
                        Id = pgrGuid
                    }
                }
            })
        };

        var expectedKgrResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new CrmBaseResponse<KundenGr>()
            {
                Value = Array.Empty<KundenGr>(),
            })
        };

        var expectedPgrGuidUri = new Uri($"http://localhost/api/data/v9.1/pgrs?$filter=nummer+eq+{pgrNummer}&$select=id");

        var expectedKgrUri = new Uri($"http://localhost/api/data/v9.1/kgrs?$filter=_pgr_id+eq+%27{pgrGuid}%27&$select=id%2c_regionsArt_1%2c_regionsArt_2");

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedPgrGuidUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedPgrGuidResponse));

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedKgrUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedKgrResponse));

        var result = await _crmService.GetAlleKGRsFürPgr(pgrNummer);

        Assert.Empty(result);
    }

    [Fact]
    public async Task FürEineStreckeOhneGuidWirdEineLeereListeZurückgegeben()
    {
        var pgrNummer = 123;

        var expectedResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new CrmBaseResponse<PreisGr>()
            {
                Value = Array.Empty<PreisGr>()
            })
        };

        var expectedUri = new Uri($"http://localhost/api/data/v9.1/pgrs?$filter=nummer+eq+{pgrNummer}&$select=id");

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedResponse));

        var result = await _crmService.GetAlleKGRsFürPgr(pgrNummer);

        Assert.Empty(result);
    }
}
