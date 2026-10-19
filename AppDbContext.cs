using Microsoft.EntityFrameworkCore;
using RACE_DAY_.Controllers;
using RACE_DAY_.Models;

namespace RACE_DAY_.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder b)
    {
       
        b.Entity<Organizer>().HasIndex(o => o.Email).IsUnique();
        b.Entity<Participant>().HasIndex(p => p.Email).IsUnique();
        b.Entity<Registration>().HasIndex(r => new { r.ParticipantId, r.CategoryId }).IsUnique();
        b.Entity<Result>().HasIndex(r => r.RegistrationId).IsUnique();

        b.Entity<Category>().ToTable("Category", t => t.HasCheckConstraint("CK_Category_Distance", "[Distance] > 0"));
        b.Entity<Result>().ToTable("Result", t => t.HasCheckConstraint("CK_Result_Position", "[Position] > 0"));

        b.Entity<Registration>().Property(r => r.PaymentStatus).HasDefaultValue("Pending");
        b.Entity<Registration>().Property(r => r.RegistrationDate).HasDefaultValueSql("GETDATE()");
        b.Entity<Event>().Property(e => e.EventType).HasConversion<string>().HasMaxLength(10);

       
        foreach (var fk in b.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}