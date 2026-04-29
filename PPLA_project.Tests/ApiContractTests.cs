using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

public class ApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ApiContractTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Fuel_InvalidInput_ShouldReturnBadRequest()
    {
        var res = await _client.PostAsJsonAsync("/api/fuel", new { distanceNm = 0, trueAirspeedKt = 100, windComponentKt = 0, fuelBurnPerHourL = 20 });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MetarV2_ShouldReturnIssues()
    {
        var res = await _client.PostAsJsonAsync("/api/metar/v2/parse", new { raw = "METAR EPWA 291200Z SCT030" });
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await res.Content.ReadAsStringAsync();
        json.Should().Contain("issues");
    }
}
