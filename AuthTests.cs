using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Tests;


public class AuthTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _f;
    public AuthTests(ApiFactory f) => _f = f;

    [Fact]
    public async Task Register_Participant_Returns201()
    {
        var c = _f.CreateClient();
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new RegisterDto("Ann", "Lee", Api.UniqueEmail(), Api.Password, "Participant", null, new DateOnly(1990, 5, 1), null, null), Api.J);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task Register_Organizer_WithCorrectCode_Returns201()
    {
        var c = _f.CreateClient();
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new RegisterDto("Olive", "Org", Api.UniqueEmail(), Api.Password, "Organiser", null, null, "Org", Api.OrganiserCode), Api.J);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task Register_Organizer_WithoutCode_ReturnsForbidden()
    {
        var c = _f.CreateClient();
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new RegisterDto("Sneaky", "Person", Api.UniqueEmail(), Api.Password, "Organiser", null, null, "Org", null), Api.J);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        var c = _f.CreateClient();
        var r = await c.PostAsJsonAsync("/api/auth/register",
            new RegisterDto("A", "B", Api.UniqueEmail(), Api.Password, "Admin", null, null, null, null), Api.J);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var c = _f.CreateClient();
        var email = Api.UniqueEmail();
        var dto = new RegisterDto("A", "B", email, Api.Password, "Participant", null, new DateOnly(1990, 1, 1), null, null);
        await c.PostAsJsonAsync("/api/auth/register", dto, Api.J);
        var second = await _f.CreateClient().PostAsJsonAsync("/api/auth/register", dto, Api.J);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Password_IsStoredHashed_NotPlainText()
    {
        var email = Api.UniqueEmail();
        await _f.CreateClient().PostAsJsonAsync("/api/auth/register",
            new RegisterDto("A", "B", email, Api.Password, "Participant", null, new DateOnly(1990, 1, 1), null, null), Api.J);

        using var scope = _f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var p = await db.Participants.FirstAsync(x => x.Email == email);
        Assert.NotEqual(Api.Password, p.PasswordHash);
        Assert.False(string.IsNullOrWhiteSpace(p.PasswordHash));
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsOk()
    {
        var email = Api.UniqueEmail();
        await _f.CreateClient().PostAsJsonAsync("/api/auth/register",
            new RegisterDto("A", "B", email, Api.Password, "Participant", null, new DateOnly(1990, 1, 1), null, null), Api.J);

        var r = await _f.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginDto(email, Api.Password), Api.J);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = Api.UniqueEmail();
        await _f.CreateClient().PostAsJsonAsync("/api/auth/register",
            new RegisterDto("A", "B", email, Api.Password, "Participant", null, new DateOnly(1990, 1, 1), null, null), Api.J);

        var r = await _f.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginDto(email, "WrongPassword!"), Api.J);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Profile_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        var r = await _f.CreateClient().GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Profile_WhenLoggedIn_ReturnsOwnProfile()
    {
        var c = await Api.NewParticipant(_f);
        var r = await c.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var profile = await r.Content.ReadFromJsonAsync<ProfileDto>(Api.J);
        Assert.Equal("Participant", profile!.Role);
    }

    [Fact]
    public async Task Profile_Update_ChangesPhone()
    {
        var c = await Api.NewParticipant(_f);
        var r = await c.PutAsJsonAsync("/api/users/me", new UpdateProfileDto(null, null, "0831234567", null), Api.J);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var profile = await r.Content.ReadFromJsonAsync<ProfileDto>(Api.J);
        Assert.Equal("0831234567", profile!.Phone);
    }
}
