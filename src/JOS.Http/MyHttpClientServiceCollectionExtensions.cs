using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JOS.Http;

public static class MyHttpClientServiceCollectionExtensions
{
    public static IServiceCollection AddMyHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<MyHttpClient>((provider, client) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri("https://api.local.localhost");
            client.Timeout = TimeSpan.FromSeconds(5);
            var username = configuration.GetRequiredValue<string>("MyHttpClient:Username");
            var password = configuration.GetRequiredValue<string>("MyHttpClient:Password");
            var authorizationHeaderValue = $"{username}:{password}";
            var base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationHeaderValue));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Encoded);
        });
        return services;
    }
}
