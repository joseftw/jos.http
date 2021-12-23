using System.Threading.Tasks;
using JOS.Http.Asp;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace JOS.Http.Tests;

public class HttpContextCorrelationIdQueryTests
{
    private readonly HeaderDictionary _requestHeaders;
    private readonly IHttpContextAccessor _fakeHttpContextAccessor;
    private readonly HttpContextCorrelationIdQuery _sut;

    public HttpContextCorrelationIdQueryTests()
    {
        _requestHeaders = new HeaderDictionary();
        _fakeHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _fakeHttpContextAccessor.HttpContext.Request.Headers.Returns(_requestHeaders);
        _sut = new HttpContextCorrelationIdQuery(_fakeHttpContextAccessor);
    }

    [Fact]
    public async Task ShouldReturnNullIfNoCorrelationIdHeaderIsProvided()
    {
        var result = await _sut.Execute();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldReturnNullIfCorrelationIdHeaderIsLongerThan32Chars()
    {
        _requestHeaders.Add(Headers.CorrelationId, new string('a', 33));

        var result = await _sut.Execute();

        result.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ShouldReturnNullIfCorrelationIdHeaderValueIsNullOrEmpty(string headerValue)
    {
        _requestHeaders.Add(Headers.CorrelationId, headerValue);

        var result = await _sut.Execute();

        result.ShouldBeNull();
    }
}
