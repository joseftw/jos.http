using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JOS.Http.Tests;

public class FakeHttpMessageHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>>? _responseFunc;
    private readonly HttpResponseMessage? _httpResponseMessage;
    private readonly HttpStatusCode _defaultStatusCode;

    public FakeHttpMessageHandler()
    {
        _defaultStatusCode = HttpStatusCode.OK;
        _httpResponseMessage = null!;
    }

    public FakeHttpMessageHandler(HttpResponseMessage httpResponseMessage)
    {
        _httpResponseMessage = httpResponseMessage ?? throw new ArgumentNullException(nameof(httpResponseMessage));
    }

    public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFunc)
    {
        _httpResponseMessage = null!;
        _responseFunc = responseFunc;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_responseFunc != null)
        {
            return await _responseFunc(request);
        }

        return _httpResponseMessage ?? new HttpResponseMessage(_defaultStatusCode);
    }
}