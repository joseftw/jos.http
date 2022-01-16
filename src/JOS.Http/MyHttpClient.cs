using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace JOS.Http;

public class MyHttpClient
{
    private readonly HttpClient _httpClient;

    public MyHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<HttpStatusCode> HealthCheck()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        using var response = await _httpClient.SendAsync(request);
        return response.StatusCode;
    }
}
