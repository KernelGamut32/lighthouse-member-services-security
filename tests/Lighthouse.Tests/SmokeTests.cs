using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lighthouse.Tests;

public sealed class SmokeTests
{
    [Fact]
    public async Task Health_WhenApplicationStarts_ReturnsOk()
    {
        await using WebApplicationFactory<Program> factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        Assert.Contains("Lighthouse", await response.Content.ReadAsStringAsync());
    }
}
