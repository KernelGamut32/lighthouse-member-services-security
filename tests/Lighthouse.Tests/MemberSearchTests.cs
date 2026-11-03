using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lighthouse.Tests;

public sealed class MemberSearchTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    public MemberSearchTests(WebApplicationFactory<Program> factory) => client = factory.CreateClient();

    [Fact]
    public async Task SearchByName_KnownSyntheticName_ReturnsMatchingMembers()
    {
        using var response = await client.GetAsync("/api/members/search?name=Avery");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.NotEmpty(body.RootElement.EnumerateArray());
        Assert.All(body.RootElement.EnumerateArray(), x => Assert.Contains("Avery", x.GetProperty("fullName").GetString()));
    }

    [Fact]
    public async Task SearchByName_QuoteCharacters_TreatsInputAsData()
    {
        // Harmless fixture: the name does not exist and should not change query structure.
        string query = Uri.EscapeDataString("O'Imaginary");
        using var response = await client.GetAsync("/api/members/search?name=" + query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(body.RootElement.EnumerateArray());
    }
}
