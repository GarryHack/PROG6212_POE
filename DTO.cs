using RACE_DAY_.Models;

using System.ComponentModel.DataAnnotations;

namespace RACE_DAY_.DTO;

// mention in git that you added the 

// ---------- IN ----------
public record RegisterDto(
    [Required] string FirstName, [Required] string LastName,
    [Required, EmailAddress] string Email, [Required, MinLength(8)] string Password,
    [Required] string Role,                 // "Organiser" or "Participant"
    string? Phone, DateOnly? Dob,           // Dob required for Participants
    string? OrganisationName, string? OrganiserCode);   // OrganiserCode required for Organisers

public record LoginDto([Required] string Email, [Required] string Password);

public record UpdateProfileDto(string? FirstName, string? LastName, string? Phone, DateOnly? Dob);

public record EventInputDto(
    [Required] string Name, [Required] string Location, DateOnly Date,
    string? Description, [Range(0.01, 100000)] decimal Distance, EventType EventType);

public record EventUpdateDto(
    string? Name, string? Location, DateOnly? Date, string? Description,
    decimal? Distance, EventType? EventType);

public record CategoryInputDto([Required] string Name, [Range(0.01, 100000)] decimal Distance, int? AgeLimit);
public record CategoryUpdateDto(string? Name, decimal? Distance, int? AgeLimit);

public record ResultInputDto(TimeSpan FinishTime, [Range(1, int.MaxValue)] int Position, string? WeatherInfo);

// ---------- OUT ----------
public record ProfileDto(int Id, string Role, string Name, string Email, string? Phone,
                         DateOnly? Dob, string? OrganisationName);
public record EventDto(int Id, string Name, string Location, DateOnly Date, string? Description,
                       decimal Distance, EventType EventType, int OrganiserId);
public record CategoryDto(int Id, int EventId, string Name, decimal Distance, int? AgeLimit);
public record EnrolmentDto(int Id, int ParticipantId, string ParticipantName, int CategoryId,
                           string CategoryName, int EventId, string EventName,
                           DateOnly RegistrationDate, string PaymentStatus);
public record ResultDto(int Id, int RegistrationId, string ParticipantName, int EventId,
                        string EventName, string CategoryName, TimeSpan FinishTime,
                        int Position, string? WeatherInfo);
public record WeatherForecastDto(string Location, DateOnly Date, double? MaxTempC,
                                 double? MinTempC, double? PrecipitationMm);
