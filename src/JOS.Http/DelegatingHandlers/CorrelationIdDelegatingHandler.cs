using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JOS.Http.DelegatingHandlers;

public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private readonly ICorrelationIdQuery _correlationIdQuery;

    public CorrelationIdDelegatingHandler(ICorrelationIdQuery correlationIdQuery)
    {
        _correlationIdQuery = correlationIdQuery ?? throw new ArgumentNullException(nameof(correlationIdQuery));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = await _correlationIdQuery.Execute();
        if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(CorrelationIdHeaderName))
        {
            request.Headers.TryAddWithoutValidation(CorrelationIdHeaderName, correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
