using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UmbCheckout.Core.Extensions;
using Xunit;

namespace UmbCheckout.Tests.Sessions;

public sealed class UmbCheckoutSessionMiddlewareTests
{
    private static async Task<HttpClient> Client(bool siteAlsoCallsUseSession)
    {
        var host = await new HostBuilder().ConfigureWebHost(web => web
            .UseTestServer()
            .ConfigureServices(s => s.AddDistributedMemoryCache().AddSession())
            .Configure(app =>
            {
                if (siteAlsoCallsUseSession)
                {
                    app.UseSession();
                }

                app.UseUmbCheckoutSession();
                app.Run(async context =>
                {
                    var count = (context.Session.GetInt32("n") ?? 0) + 1;
                    context.Session.SetInt32("n", count);
                    await context.Response.WriteAsync($"{count}|{context.Features.Get<ISessionFeature>() is not null}");
                });
            })).StartAsync();
        return host.GetTestClient();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_session_works_with_or_without_the_sites_own_UseSession(bool siteAlsoCallsUseSession)
    {
        var client = await Client(siteAlsoCallsUseSession);

        var first = await client.GetAsync("/");
        var cookie = first.Headers.GetValues("Set-Cookie").First().Split(';')[0];
        var second = new HttpRequestMessage(HttpMethod.Get, "/");
        second.Headers.Add("Cookie", cookie);
        var secondResponse = await client.SendAsync(second);

        Assert.Equal("1|True", await first.Content.ReadAsStringAsync());
        Assert.Equal("2|True", await secondResponse.Content.ReadAsStringAsync());
    }
}
