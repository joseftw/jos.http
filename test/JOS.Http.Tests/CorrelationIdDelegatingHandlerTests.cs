using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JOS.Http.DelegatingHandlers;
using NSubstitute;
using Shouldly;
using Xunit;

namespace JOS.Http.Tests;

public class CorrelationIdDelegatingHandlerTests
{
    private readonly ICorrelationIdQuery _fakeCorrelationIdQuery;
    private readonly FakeHttpMessageHandler _fakeHttpMessageHandler;
    private readonly CorrelationIdDelegatingHandler _sut;

    public CorrelationIdDelegatingHandlerTests()
    {
        _fakeCorrelationIdQuery = Substitute.For<ICorrelationIdQuery>();
        _fakeCorrelationIdQuery.Execute().Returns(Guid.NewGuid().ToString());
        _fakeHttpMessageHandler = new FakeHttpMessageHandler();
        _sut = new CorrelationIdDelegatingHandler(_fakeCorrelationIdQuery)
        {
            InnerHandler = _fakeHttpMessageHandler
        };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ShouldNotAppendCorrelationIdHeaderIfNoCorrelationIdIsProvided(string correlationId)
    {
        _fakeCorrelationIdQuery.Execute().Returns(correlationId);
        var request = new HttpRequestMessage(HttpMethod.Get, "/correlation-id");

        _ = await new HttpMessageInvoker(_sut).SendAsync(request, CancellationToken.None);

        request.Headers.ShouldNotContain(x => x.Key.Equals("X-Correlation-Id", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ShouldNotOverwriteExistingCorrelationIdHeader()
    {
        var correlationId = Guid.NewGuid().ToString();
        _fakeCorrelationIdQuery.Execute().Returns(correlationId);
        var request = new HttpRequestMessage(HttpMethod.Get, "/correlation-id")
        {
            Headers = {{ "X-Correlation-Id", "existing-correlation-id" }}
        };

        _ = await new HttpMessageInvoker(_sut).SendAsync(request, CancellationToken.None);

        request.Headers.TryGetValues("X-Correlation-Id", out var correlationIdHeaderValues).ShouldBeTrue();
        var correlationIdValueList = correlationIdHeaderValues!.ToList();
        correlationIdValueList.Count.ShouldBe(1);
        correlationIdValueList.First().ShouldBe("existing-correlation-id");
    }
    
    [Fact]
    public async Task ShouldAppendCorrelationIdHeaderIfProvided()
    {
        var correlationId = Guid.NewGuid().ToString();
        _fakeCorrelationIdQuery.Execute().Returns(correlationId);
        var request = new HttpRequestMessage(HttpMethod.Get, "/correlation-id");

        _ = await new HttpMessageInvoker(_sut).SendAsync(request, CancellationToken.None);
        
        request.Headers.TryGetValues("X-Correlation-Id", out var correlationIdHeaderValues).ShouldBeTrue();
        var correlationIdValueList = correlationIdHeaderValues!.ToList();
        correlationIdValueList.Count.ShouldBe(1);
        correlationIdValueList.First().ShouldBe(correlationId);
    }
}
