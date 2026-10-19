using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Tests;

public class EnrolmentAndResultTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _f;
    public EnrolmentAndResultTests(ApiFactory f) => _f = f;

    [Fact]
    public async Task Participant_CanEnrol_AndEnrolmentIsRecorded()
    {
        var org = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        var p = await Api.NewParticipant(_f);

        var r = await p.PostAsync($"/api/categories/{cat.Id}/enrolments", null);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);

        // 1) visible through the API
        var mine = await (await p.GetAsync("/api/users/me/enrolments")).Content.ReadFromJsonAsync<List<EnrolmentDto>>(Api.J);
        Assert.Single(mine!);
        Assert.Equal(cat.Id, mine![0].CategoryId);

        // 2) really saved in the database
        using var scope = _f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await db.Registrations.CountAsync(x => x.CategoryId == cat.Id));
    }

    [Fact]
    public async Task Participant_CannotEnrolTwiceInSameCategory_ReturnsConflict()
    {
        var org = await Api.NewOrganiser(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        var p = await Api.NewParticipant(_f);
        await Api.Enrol(p, cat.Id);
        var second = await p.PostAsync($"/api/categories/{cat.Id}/enrolments", null);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Underage_Participant_CannotEnrol_ReturnsBadRequest()
    {
        var org = await Api.NewOrganiser(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id, ageLimit: 18);
        var kid = await Api.NewParticipant(_f, DateOnly.FromDateTime(DateTime.Today.AddYears(-10)));
        var r = await kid.PostAsync($"/api/categories/{cat.Id}/enrolments", null);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Enrol_InMissingCategory_ReturnsNotFound()
    {
        var p = await Api.NewParticipant(_f);
        var r = await p.PostAsync("/api/categories/999999/enrolments", null);
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Organizer_CannotEnrol_ReturnsForbidden()
    {
        var org = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        var r = await org.PostAsync($"/api/categories/{cat.Id}/enrolments", null);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task NotLoggedIn_CannotEnrol_ReturnsUnauthorized()
    {
        var org = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        var r = await _f.CreateClient().PostAsync($"/api/categories/{cat.Id}/enrolments", null);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanViewEnrolmentsForOwnCategory()
    {
        var org = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        await Api.Enrol(await Api.NewParticipant(_f), cat.Id);

        var r = await org.GetAsync($"/api/categories/{cat.Id}/enrolments");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var list = await r.Content.ReadFromJsonAsync<List<EnrolmentDto>>(Api.J);
        Assert.Single(list!);
    }

    [Fact]
    public async Task Organizer_CannotViewEnrolmentsOfAnotherOrganisersCategory_ReturnsForbidden()
    {
        var owner = await Api.NewOrganizer(_f);
        var other = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(owner, (await Api.CreateEvent(owner)).Id);
        var r = await other.GetAsync($"/api/categories/{cat.Id}/enrolments");
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewCategoryEnrolments_ReturnsForbidden()
    {
        var org = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        var p = await Api.NewParticipant(_f);
        var r = await p.GetAsync($"/api/categories/{cat.Id}/enrolments");
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanRecordResult_AndParticipantCanSeeIt()
    {
        var org = await Api.NewOrganizer(_f);
        var ev = await Api.CreateEvent(org);
        var cat = await Api.CreateCategory(org, ev.Id);
        var p = await Api.NewParticipant(_f);
        var enrolment = await Api.Enrol(p, cat.Id);

        var r = await org.PostAsJsonAsync($"/api/enrolments/{enrolment.Id}/results",
            new ResultInputDto(TimeSpan.FromMinutes(45), 1, "Sunny, 22C"), Api.J);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);

        var mine = await (await p.GetAsync("/api/users/me/results")).Content.ReadFromJsonAsync<List<ResultDto>>(Api.J);
        Assert.Single(mine!);
        Assert.Equal(1, mine![0].Position);

        var board = await (await _f.CreateClient().GetAsync($"/api/events/{ev.Id}/results")).Content.ReadFromJsonAsync<List<ResultDto>>(Api.J);
        Assert.Single(board!);
    }

    [Fact]
    public async Task Participant_CannotRecordResult_ReturnsForbidden()
    {
        var org = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        var p = await Api.NewParticipant(_f);
        var enrolment = await Api.Enrol(p, cat.Id);
        var r = await p.PostAsJsonAsync($"/api/enrolments/{enrolment.Id}/results",
            new ResultInputDto(TimeSpan.FromMinutes(30), 1, null), Api.J);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotRecordResultForSomeoneElsesEvent_ReturnsForbidden()
    {
        var owner = await Api.NewOrganizer(_f);
        var other = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(owner, (await Api.CreateEvent(owner)).Id);
        var enrolment = await Api.Enrol(await Api.NewParticipant(_f), cat.Id);
        var r = await other.PostAsJsonAsync($"/api/enrolments/{enrolment.Id}/results",
            new ResultInputDto(TimeSpan.FromMinutes(30), 1, null), Api.J);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Organizer_CannotRecordTwoResultsForSameEnrolment_ReturnsConflict()
    {
        var org = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        var enrolment = await Api.Enrol(await Api.NewParticipant(_f), cat.Id);
        var body = new ResultInputDto(TimeSpan.FromMinutes(30), 1, null);
        await org.PostAsJsonAsync($"/api/enrolments/{enrolment.Id}/results", body, Api.J);
        var second = await org.PostAsJsonAsync($"/api/enrolments/{enrolment.Id}/results", body, Api.J);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Record_InvalidPosition_ReturnsBadRequest()
    {
        var org = await Api.NewOrganizer(_f);
        var cat = await Api.CreateCategory(org, (await Api.CreateEvent(org)).Id);
        var enrolment = await Api.Enrol(await Api.NewParticipant(_f), cat.Id);
        var r = await org.PostAsJsonAsync($"/api/enrolments/{enrolment.Id}/results",
            new ResultInputDto(TimeSpan.FromMinutes(30), 0, null), Api.J);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }
}
