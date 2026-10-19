using RACE_DAY_.Controllers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACE_DAY_.Models
{
    [Table("Event")]
    public class Event
    {
        [Key, Column("event_id")]
        public int Id { get; set; }

        [Column("Event_Name"), Required, MaxLength(150)]
        public string Name { get; set; } = "";

        [Required, MaxLength(150)]
        public string Location { get; set; } = "";

        [Column("Event_Date")]
        public DateOnly Date { get; set; }

        [Column("Event_Description"), MaxLength(500)]
        public string? Description { get; set; }

        [Column("Event_Type")]
        public EventType EventType { get; set; }         

        [Column("Distance", TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }             
        [Column("Organiser_id")]
        public int OrganizerId { get; set; }              // foreign key column
        public Organizer? Organiser { get; set; }         // who owns this event?

        public List<Category> Categories { get; set; } = new();
    }

}
