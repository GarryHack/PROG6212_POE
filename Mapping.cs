using Microsoft.EntityFrameworkCore;
using RACE_DAY_.DTO;
using RACE_DAY_.Models;


namespace RaceDay.Api.Services;

/// <summary>Turns database entities into the safe DTOs we send back as JSON.</summary>
public static class Mapping
{
    public static ProfileDto ToProfile(this Organizer o) =>
        new(o.Id, "Organiser", o.Name, o.Email, o.Phone, null, o.OrganisationName);

    public static ProfileDto ToProfile(this Participant p) =>
        new(p.Id, "Participant", $"{p.FirstName} {p.LastName}", p.Email, p.Phone, p.Dob, null);

    public static EventDto ToDto(this Event e) =>
        new(e.Id, e.Name, e.Location, e.Date, e.Description, e.Distance, e.EventType, e.OrganizerId);

    public static CategoryDto ToDto(this Category c) =>
        new(c.Id, c.EventId, c.Name, c.Distance, c.AgeLimit);

    public static EnrolmentDto ToDto(this Registration r) =>
        new(r.Id, r.ParticipantId, $"{r.Participant!.FirstName} {r.Participant.LastName}",
            r.CategoryId, r.Category!.Name, r.Category.EventId, r.Category.Event!.Name,
            r.RegistrationDate, r.PaymentStatus);

    public static ResultDto ToDto(this Result x) =>
        new(x.Id, x.RegistrationId,
            $"{x.Registration!.Participant!.FirstName} {x.Registration.Participant.LastName}",
            x.Registration.Category!.EventId, x.Registration.Category.Event!.Name,
            x.Registration.Category.Name, x.FinishTime, x.Position, x.WeatherInfo);

    // "Include" tells EF to also load the related rows (participant, category, event).
    public static IQueryable<Registration> WithDetails(this IQueryable<Registration> q) =>
        q.Include(r => r.Participant).Include(r => r.Category).ThenInclude(c => c!.Event);

    public static IQueryable<Result> WithDetails(this IQueryable<Result> q) =>
        q.Include(x => x.Registration).ThenInclude(r => r!.Participant)
         .Include(x => x.Registration).ThenInclude(r => r!.Category).ThenInclude(c => c!.Event);
}
