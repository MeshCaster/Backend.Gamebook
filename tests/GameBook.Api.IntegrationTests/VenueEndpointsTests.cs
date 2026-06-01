using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GameBook.Contracts.Venues;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GameBook.Api.IntegrationTests;

public class VenueEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public VenueEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetVenues_ReturnsOk()
    {
        var response = await _client.GetAsync("/v1/venues");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetVenueBySlug_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/v1/venues/non-existent-venue");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
