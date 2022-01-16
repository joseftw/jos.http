using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace JOS.Http.Tests;

public class MyHttpClientServiceCollectionExtensionsTests
{
    [Fact]
    public void MyHttpClient_ShouldSetBaseBaseAddressWhenRegistering()
    {
        var configuration = CreateConfiguration(new List<KeyValuePair<string, string>>
        {
            new("MyHttpClient:Username", "any-username"),
            new("MyHttpClient:Password", "any-password")
        });
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMyHttpClient();
        
        var serviceProvider = services.BuildServiceProvider();
        var myHttpClient = serviceProvider.GetRequiredService<MyHttpClient>();
        var httpClient = GetHttpClientField(myHttpClient);
        httpClient.ShouldNotBeNull();
        httpClient.ShouldBeOfType<HttpClient>();
        httpClient.BaseAddress.ShouldBe(new Uri("https://api.local.localhost"));
    }

    [Fact]
    public void MyHttpClient_ShouldThrowExceptionIfMyHttpClientUsernameConfigurationIsMissing()
    {
        var configuration = CreateConfiguration(new List<KeyValuePair<string, string>>
        {
            new("MyHttpClient:Password", "any-password")
        });
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMyHttpClient();

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<Exception>(() => serviceProvider.GetRequiredService<MyHttpClient>());
        exception.Message.ShouldBe("'MyHttpClient:Username' had no value, have you forgot to add it to the Configuration?");
    }

    [Fact]
    public void MyHttpClient_ShouldThrowExceptionIfMyHttpClientPasswordConfigurationIsMissing()
    {
        var configuration = CreateConfiguration(new List<KeyValuePair<string, string>>
        {
            new("MyHttpClient:Username", "any-username")
        });
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMyHttpClient();

        var serviceProvider = services.BuildServiceProvider();
        var exception = Should.Throw<Exception>(() => serviceProvider.GetRequiredService<MyHttpClient>());
        exception.Message.ShouldBe("'MyHttpClient:Password' had no value, have you forgot to add it to the Configuration?");
    }

    [Fact]
    public void MyHttpClient_ShouldSetTimeoutTo5Seconds()
    {
        var username = "any-username";
        var password = "any-password";
        var configuration = CreateConfiguration(new List<KeyValuePair<string, string>>
        {
            new("MyHttpClient:Username", username),
            new("MyHttpClient:Password", password)
        });
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var expectedAuthorizationHeaderValue =
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

        services.AddMyHttpClient();

        var serviceProvider = services.BuildServiceProvider();
        var myHttpClient = serviceProvider.GetRequiredService<MyHttpClient>();
        var httpClient = GetHttpClientField(myHttpClient);
        httpClient.ShouldNotBeNull();
        httpClient.Timeout.ShouldBe(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MyHttpClient_ShouldAddBase64EncodedBasicAuthorizationHeader()
    {
        var username = "any-username";
        var password = "any-password";
        var configuration = CreateConfiguration(new List<KeyValuePair<string, string>>
        {
            new("MyHttpClient:Username", username),
            new("MyHttpClient:Password", password)
        });
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        var expectedAuthorizationHeaderValue =
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

        services.AddMyHttpClient();
        
        var serviceProvider = services.BuildServiceProvider();
        var myHttpClient = serviceProvider.GetRequiredService<MyHttpClient>();
        var httpClient = GetHttpClientField(myHttpClient);
        httpClient.ShouldNotBeNull();
        httpClient.DefaultRequestHeaders.Authorization.ShouldNotBeNull();
        httpClient.DefaultRequestHeaders.Authorization.Scheme.ShouldBe("Basic");
        httpClient.DefaultRequestHeaders.Authorization.Parameter.ShouldBe(expectedAuthorizationHeaderValue);
    }

    private static ConfigurationRoot CreateConfiguration(
        List<KeyValuePair<string, string>> configurationValues)
    {
        return new ConfigurationRoot(new List<IConfigurationProvider>
        {
            new MemoryConfigurationProvider(
                new MemoryConfigurationSource
                {
                    InitialData = configurationValues
                })
        });
    }

    private static HttpClient GetHttpClientField(MyHttpClient myHttpClient)
    {
        return myHttpClient
               .GetType()
               .GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic)
               .FirstOrDefault(x => x.FieldType == typeof(HttpClient))
               ?.GetValue(myHttpClient) as HttpClient
               ?? throw new Exception("Failed to find a HttpClient field!");
    }
}
