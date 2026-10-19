using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Tests;


public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // remove the SQL Server database registration...
            var old = services.Where(d => d.ServiceType.IsGenericType &&
                                          d.ServiceType.GetGenericArguments().Contains(typeof(AppDbContext))).ToList();
            foreach (var d in old) services.Remove(d);
            // ...and use an in-memory one instead
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            // never call the internet from a test
            foreach (var d in services.Where(d => d.ServiceType == typeof(IWeatherService)).ToList()) services.Remove(d);
            services.AddSingleton<IWeatherService, FakeWeather>();
        });
    }
}

public class FakeWeather : IWeatherService
{
    public Task<WeatherForecastDto?> GetForecastAsync(string location, DateOnly date) =>
        Task.FromResult<WeatherForecastDto?>(new WeatherForecastDto(location, date, 25, 15, 0));
}

public static class Api
{
    // Same JSON rules as the API (snake_case + enums as text)
    public static readonly JsonSerializerOptions J = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() }
    };

    public const string OrganiserCode = "RACEDAY-ORG-2026";   // must match appsettings.json
    public const string Password = "Password123!";

    public static string UniqueEmail() => $"{Guid.NewGuid():N}@test.com";
    public static DateOnly InDays(int n) => DateOnly.FromDateTime(DateTime.Today.AddDays(n));

    /// 
    public static async Task<HttpClient> NewOrganiser(ApiFactory f)
    {
        var c = f.CreateClient();   // each client keeps its own cookies = its own login session
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new RegisterDto("Olive", "Organiser", UniqueEmail(), Password, "Organiser", null, null, "Test Org", OrganiserCode), J);
        r.EnsureSuccessStatusCode();
        return c;
    }

   
    public static async Task<HttpClient> NewParticipant(ApiFactory f, DateOnly? dob = null)
    {
        var c = f.CreateClient();
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new RegisterDto("Pat", "Participant", UniqueEmail(), Password, "Participant", null, dob ?? new DateOnly(1995, 1, 1), null, null), J);
        r.EnsureSuccessStatusCode();
        return c;
    }

    public static async Task<EventDto> CreateEvent(HttpClient organiser)
    {
        var r = await organiser.PostAsJsonAsync("/api/events",
            new EventInputDto("Test Run", "Durban", InDays(30), "A test event", 10m, EventType.Run), J);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<EventDto>(J))!;
    }

    public static async Task<CategoryDto> CreateCategory(HttpClient organiser, int eventId, int? ageLimit = null)
    {
        var r = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories",
            new CategoryInputDto("10km", 10m, ageLimit), J);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<CategoryDto>(J))!;
    }

    public static async Task<EnrolmentDto> Enrol(HttpClient participant, int categoryId)
    {
        var r = await participant.PostAsync($"/api/categories/{categoryId}/enrolments", null);
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<EnrolmentDto>(J))!;
    }
}
