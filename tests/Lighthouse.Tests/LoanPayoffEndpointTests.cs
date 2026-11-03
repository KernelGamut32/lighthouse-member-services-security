using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lighthouse.Tests;

public sealed class LoanPayoffEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    public LoanPayoffEndpointTests(WebApplicationFactory<Program> factory) => client = factory.CreateClient();

    [Theory]
    [InlineData("/api/loans/L0001/payoff")]
    [InlineData("/api/loans/L0001/payoff?payoffDate=")]
    [InlineData("/api/loans/L0001/payoff?payoffDate=not-a-date")]
    [InlineData("/api/loans/L0001/payoff?payoffDate=2026-02-30")]
    [InlineData("/api/loans/L0001/payoff?payoffDate=10%2F31%2F2026")]
    [InlineData("/api/loans/bad/payoff?payoffDate=2026-10-31")]
    [InlineData("/api/loans/l0001/payoff?payoffDate=2026-10-31")]
    public async Task GetPayoff_InvalidRequest_Returns400ProblemDetails(string path)
    {
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, body.RootElement.GetProperty("status").GetInt32());
        Assert.True(body.RootElement.TryGetProperty("title", out _));
    }

    [Fact]
    public async Task GetPayoff_UnknownValidId_Returns404ProblemDetails()
    {
        using var response = await client.GetAsync("/api/loans/L9999/payoff?payoffDate=2026-10-31");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("L0299")]
    [InlineData("L0300")]
    public async Task GetPayoff_IneligibleLoan_Returns409ProblemDetails(string id)
    {
        using var response = await client.GetAsync($"/api/loans/{id}/payoff?payoffDate=2026-10-31");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetPayoff_EligibleLoan_ReturnsExistingQuoteShape()
    {
        using var response = await client.GetAsync("/api/loans/L0001/payoff?payoffDate=2026-10-31");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("L0001", body.RootElement.GetProperty("loanId").GetString());
        Assert.Equal("M0001", body.RootElement.GetProperty("memberId").GetString());
        Assert.Equal(JsonValueKind.Number, body.RootElement.GetProperty("totalPayoff").ValueKind);
        Assert.Equal(JsonValueKind.Array, body.RootElement.GetProperty("calculationTrace").ValueKind);
    }

    [Fact]
    public async Task GetPayoff_BeforeInterestAsOf_RemainsAllowed()
    {
        using var response = await client.GetAsync("/api/loans/L0001/payoff?payoffDate=2026-09-30");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, body.RootElement.GetProperty("interestDays").GetInt32());
    }
}
