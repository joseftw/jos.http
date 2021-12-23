using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JOS.Http.Tests;

public class DummyHttpClient
{
    private readonly HttpClient _httpClient;

    public DummyHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> GetData()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/data");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
