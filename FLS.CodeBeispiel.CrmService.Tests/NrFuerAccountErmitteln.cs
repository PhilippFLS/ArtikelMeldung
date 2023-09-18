using Microsoft.Extensions.Logging;
using Moq;
using FLS.CodeBeispiel.CrmService.Models.Infrastructure;
using System.Net.Http.Json;
using System.Net;
using FLS.CodeBeispiel.CrmService.Models.CrmEntities;
using Moq.Protected;

namespace FLS.CodeBeispiel.CrmService.Tests;

public sealed class NrFuerAccountErmitteln : IDisposable
{
    private readonly Mock<ILogger<Infrastructure.CrmService>> _logger = new();
    private readonly Mock<HttpMessageHandler> _handler = new();
    private readonly HttpClient _httpClient = new();
    private Infrastructure.CrmService _crmService { get; set; }

    public NrFuerAccountErmitteln()
    {
        _httpClient = new HttpClient(_handler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _crmService = new Infrastructure.CrmService(_httpClient, _logger.Object);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    [Fact]
    public async Task FuerEinenAccountWirdDieNrKorrektErmittelt()
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
                        Accountid = accountGuid,
                        Accountnumber = "12345"
                    }
                }
            })
        };

        var expectedUri = new Uri($"http://localhost/api/data/v9.1/accs?$filter=accountid+eq+%27{accountGuid}%27&$select=accountnumber");

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedResponse));

        var result = await _crmService.GetNummerForAccount(accountGuid);

        Assert.Equal(12345, result);
    }

    [Fact]
    public async Task FuerEinenAccountOhneNrWirdNullZurückGegeben()
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
                        Accountid = accountGuid,
                        Accountnumber = null
                    }
                }
            })
        };

        var expectedUri = new Uri($"http://localhost/api/data/v9.1/accs?$filter=accountid+eq+%27{accountGuid}%27&$select=accountnumber");

        _handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(expectedResponse));

        var result = await _crmService.GetNummerForAccount(accountGuid);

        Assert.Null(result);
    }
}