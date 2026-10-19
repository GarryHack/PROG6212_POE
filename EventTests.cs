using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Tests;

public class EventTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _f;
    public EventTests(ApiFactory f) => _f = f;

    private static EventInputDto NewEvent() =>
        new("Fun Run", "Cape Town", Api.InDays(20), "desc", 5m, EventType.Run);

    [Fact]
    public async Task Organizer_CanCreateEvent_Returns201()
    {
        var org = await Api.NewOrganizer(_f);
        var r = await org.PostAsJsonAsync("/api/events", NewEvent(), Api.J);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent_ReturnsForbidden()
    {
        var p = await Api.NewParticipant(_f);
        var r = await p.PostAsJsonAsync("/api/events", NewEvent(), Api.J);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task NotLoggedIn_CannotCreateEvent_ReturnsUnauthorized()
    {
        var r = await _f.CreateClient().PostAsJsonAsync("/api/events", NewEvent(), Api.J);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Event_InThePast_ReturnsBadRequest()
    {
        var org = await Api.NewOrganizer(_f);
        var r = await org.PostAsJsonAsync("/api/events",
            new EventInputDto("Old", "X", Api.InDays(-5), null, 5m, EventType.Walk), Api.J);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Anyone_CanViewEvents_Returns200()
    {
        var org = await Api.NewOrganizer(_f);
        await Api.CreateEvent(org);
        var r = await _f.CreateClient().GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var list = await r.Content.ReadFromJsonAsync<List<EventDto>>(Api.J);
        Assert.NotEmpty(list!);
    }

    [Fact]
    public async Task Organiser_CanUpdateOwnEvent_Returns200()
    {
        var org = await Api.NewOrganizer(_f);
        var ev = await Api.CreateEvent(org);
        var r = await org.PutAsJsonAsync($"/api/events/{ev.Id}", new EventUpdateDto("Renamed", null, null, null, null, null), Api.J);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var updated = await r.Content.ReadFromJsonAsync<EventDto>(Api.J);
        Assert.Equal("Renamed", updated!.Name);
    }

    [Fact]
    public async Task Organiser_CannotUpdateAnotherOrganisersEvent_ReturnsForbidden()
    {
        var owner = await Api.NewOrganizer(_f);
        var other = await Api.NewOrganizer(_f);
        var ev = await Api.CreateEvent(owner);
        var r = await other.PutAsJsonAsync($"/api/events/{ev.Id}", new EventUpdateDto("Hacked", null, null, null, null, null), Api.J);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanDeleteOwnEvent_Returns204()
    {
        var org = await Api.NewOrganizer(_f);
        var ev = await Api.CreateEvent(org);
        var r = await org.DeleteAsync($"/api/events/{ev.Id}");
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await org.GetAsync($"/api/events/{ev.Id}")).StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentEvent_ReturnsNotFound()
    {
        var org = await Api.NewOrganizer(_f);
        var r = await org.DeleteAsync("/api/events/999999");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotDeleteEvent_ReturnsForbidden()
    {
        var org = await Api.NewOrganizer(_f);
        var ev = await Api.CreateEvent(org);
        var p = await Api.NewParticipant(_f);
        var r = await p.DeleteAsync($"/api/events/{ev.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanAddCategory_AndAnyoneCanViewIt()
    {
        var org = await Api.NewOrganizer(_f);
        var ev = await Api.CreateEvent(org);
        await Api.CreateCategory(org, ev.Id);
        var r = await _f.CreateClient().GetAsync($"/api/events/{ev.Id}/categories");
        var cats = await r.Content.ReadFromJsonAsync<List<CategoryDto>>(Api.J);
        Assert.Single(cats!);
    }

    [Fact]
    public async Task Participant_CannotAddCategory_ReturnsForbidden()
    {
        var org = await Api.NewOrganizer(_f);
        var ev = await Api.CreateEvent(org);
        var p = await Api.NewParticipant(_f);
        var r = await p.PostAsJsonAsync($"/api/events/{ev.Id}/categories", new CategoryInputDto("Hack", 5m, null), Api.J);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Weather_ForExistingEvent_ReturnsOk()
    {
        var org = await Api.NewOrganizer(_f);
        var ev = await Api.CreateEvent(org);
        var r = await _f.CreateClient().GetAsync($"/api/events/{ev.Id}/weather");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }
}
