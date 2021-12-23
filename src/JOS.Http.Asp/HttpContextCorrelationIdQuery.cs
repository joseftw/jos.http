using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JOS.Http.Asp;

public class HttpContextCorrelationIdQuery : ICorrelationIdQuery
{
    private const int MaxLength = 32;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCorrelationIdQuery(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public Task<string?> Execute()
    {
        if (!_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(
                Headers.CorrelationId,
                out var headerValues))
        {
            return Task.FromResult<string>(null!)!;
        }

        if (string.IsNullOrWhiteSpace(headerValues))
        {
            return Task.FromResult<string>(null!)!;
        }

        var correlationId = headerValues.First();

        if (correlationId.Length <= MaxLength)
        {
            return Task.FromResult(correlationId)!;
        }

        return Task.FromResult<string>(null!)!;
    }
}
