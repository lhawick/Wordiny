using System.Text.Json;
using System.Web;

namespace Wordiny.Api.Services;

internal record CityData(string Name, string FullRegion, string TimeZone);

internal interface IOxilorApiService
{
    Task<CityData[]> GetCitiesDataByNameAsync(string cityName, CancellationToken token = default);
}

internal class OxilorApiService : IOxilorApiService
{
    private readonly HttpClient _httpClient;

    public OxilorApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private record Region(string Name, string Type, string TimeZone, string[] ParentRegions);

    public async Task<CityData[]> GetCitiesDataByNameAsync(string cityName, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName, nameof(cityName));

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["searchTerm"] = cityName;
        query["type"] = "city";

        using var request = new HttpRequestMessage(HttpMethod.Get, $"search-regions?{query}");
        using var response = await _httpClient.SendAsync(request, token);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Error fetching city data from Oxilor API. Status code: {response.StatusCode}",
                null,
                response.StatusCode);
        }

        using var responseStream = await response.Content.ReadAsStreamAsync(token);

        var regionsData = JsonSerializer.Deserialize<Region[]>(responseStream);

        if (regionsData is null)
        {
            throw new Exception("Failed to deserialize response from Oxilor API");
        }

        return regionsData
            // Только точные соответствия по названию города
            .Where(rd => rd.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase))
            .Select(rd =>
            {
                // Всё кроме континента
                var parentRegion = string.Join(", ", rd.ParentRegions[..^1]);
                return new CityData(rd.Name, parentRegion, rd.TimeZone);
            })
            .ToArray();
    }
}
