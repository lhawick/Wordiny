using System.Text.Json;
using System.Web;

namespace Wordiny.Api.Services;

internal record CityData(string Name, string TimeZone);

internal interface IOxilorApiService
{
    Task<CityData[]> FindCitiesByNameAsync(string cityName, CancellationToken token = default);
}

internal class OxilorApiService : IOxilorApiService
{
    private readonly HttpClient _httpClient;

    private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public OxilorApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CityData[]> FindCitiesByNameAsync(string cityName, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName, nameof(cityName));

        var trimmedCityName = cityName.Trim();

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["searchTerm"] = trimmedCityName;
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

        await using var responseStream = await response.Content.ReadAsStreamAsync(token);

        var regionsData = JsonSerializer.Deserialize<Region[]>(responseStream, _jsonSerializerOptions);

        if (regionsData is null)
        {
            throw new Exception("Failed to deserialize response from Oxilor API");
        }

        return regionsData
            .Where(x => x.Type.Equals("city", StringComparison.OrdinalIgnoreCase))
            // Только точные соответствия по названию города
            .Where(rd => rd.Name.Equals(trimmedCityName, StringComparison.OrdinalIgnoreCase))
            .Select(rd =>
            {
                // Всё кроме континента
                var parentRegion = string.Join(", ", rd.ParentRegions[..^1]
                    .Select(r => r.Name)
                    // Если область = название города, пропускаем
                    .Where(x => !x.Equals(rd.Name, StringComparison.OrdinalIgnoreCase)));
                
                var fullCityLocation = $"{rd.Name}, {parentRegion}";

                return new CityData(fullCityLocation, rd.TimeZone);
            })
            .Take(8)
            .ToArray();
    }

    private class Region
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public ParentRegion[] ParentRegions { get; set; } = [];
    }

    private class ParentRegion
    {
        public string Name { get; set; } = string.Empty;
    }
}