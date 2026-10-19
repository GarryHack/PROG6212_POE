using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RACE_DAY_.Data;
using RACE_DAY_.Models;

//using RACE_DAY_.Dtos;
//using RACE_DAY_.Models;
using RACE_DAY_.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    /// <summary>Update a category (only the Organiser who owns its event).</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
    {
        var c = await _db.Categories.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        if (c.Event!.OrganiserId != User.UserId()) return Forbid();

        if (!string.IsNullOrWhiteSpace(dto.Name)) c.Name = dto.Name;
        if (dto.Distance != null && dto.Distance > 0) c.Distance = dto.Distance.Value;
        if (dto.AgeLimit != null) c.AgeLimit = dto.AgeLimit;
        await _db.SaveChangesAsync();
        return Ok(c.ToDto());
    }

    /// <summary>Delete a category (owner only). Blocked if participants are enrolled in it.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Categories.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        if (c.Event!.OrganiserId != User.UserId()) return Forbid();
        if (await _db.Registrations.AnyAsync(r => r.CategoryId == id))
            return Conflict("This category has enrolments and cannot be deleted.");

        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Enrol yourself in a category (Participant only). The participant comes from your login session.</summary>
    [Authorize(Roles = "Participant")]
    [HttpPost("{id:int}/enrolments")]
    [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(int id)
    {
        var me = User.UserId();
        var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == id);
        if (cat == null) return NotFound();
        var p = await _db.Participants.FindAsync(me);
        if (p == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (cat.Event!.Date < today) return BadRequest("This event has already taken place.");

        if (cat.AgeLimit is int minAge)                       // age check using the participant's date of birth
        {
            var age = today.Year - p.Dob.Year;
            if (p.Dob > today.AddYears(-age)) age--;
            if (age < minAge) return BadRequest($"You must be at least {minAge} to enter this category.");
        }

        if (await _db.Registrations.AnyAsync(r => r.ParticipantId == me && r.CategoryId == id))
            return Conflict("You are already enrolled in this category.");

        var reg = new Registration
        {
            ParticipantId = me,
            CategoryId = id,
            RegistrationDate = today,
            PaymentStatus = "Pending"
        };
        _db.Registrations.Add(reg);
        await _db.SaveChangesAsync();

        var saved = await _db.Registrations.AsNoTracking().WithDetails().FirstAsync(r => r.Id == reg.Id);
        return Created("/api/users/me/enrolments", saved.ToDto());
    }

    /// <summary>See everyone enrolled in a category (only the Organiser who owns the event).</summary>
    [Authorize(Roles = "Organiser")]
    [HttpGet("{id:int}/enrolments")]
    [ProducesResponseType(typeof(List<EnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrolments(int id)
    {
        var c = await _db.Categories.AsNoTracking().Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        if (c.Event!.OrganiserId != User.UserId()) return Forbid();

        var list = await _db.Registrations.AsNoTracking().WithDetails().Where(r => r.CategoryId == id).ToListAsync();
        return Ok(list.Select(r => r.ToDto()));
    }
}
