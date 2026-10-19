

using System.Globalization;
using System.Text.Json;
using RACE_DAY_.DTO;

public interface IWeatherService
{
    Task<WeatherForecastDto?> GetForecastAsync(string location, DateOnly date);
}

///Uses the free Open-Meteo API (no API key needed).
public class OpenMeteoWeatherService : IWeatherService
{
    private readonly HttpClient _http;
    public OpenMeteoWeatherService(HttpClient http) => _http = http;

    public async Task<WeatherForecastDto?> GetForecastAsync(string location, DateOnly date)
    {
        try
        {
            // 1) turn "Durban" into latitude/longitude
            var geoJson = await _http.GetStringAsync(
                $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(location)}&count=1");
            using var geo = JsonDocument.Parse(geoJson);
            if (!geo.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                return null;
            var lat = results[0].GetProperty("latitude").GetDouble().ToString(CultureInfo.InvariantCulture);
            var lon = results[0].GetProperty("longitude").GetDouble().ToString(CultureInfo.InvariantCulture);

            // 2) ask for that day's forecast (only works ~16 days ahead)
            var d = date.ToString("yyyy-MM-dd");
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}" +
                      $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=auto&start_date={d}&end_date={d}";
            using var fc = JsonDocument.Parse(await _http.GetStringAsync(url));
            var daily = fc.RootElement.GetProperty("daily");
            double? First(string name)
            {
                var v = daily.GetProperty(name)[0];
                return v.ValueKind == JsonValueKind.Null ? null : v.GetDouble();
            }
            return new WeatherForecastDto(location, date,
                First("temperature_2m_max"), First("temperature_2m_min"), First("precipitation_sum"));
        }
        catch (Exception)   // date too far away, no internet, unknown city etc.
        {
            return null;
        }
    }
}
//