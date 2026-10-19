using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACE_DAY_.Models
{
    [Table("Organizer")]
    public class Organizer
    {
        [Key, Column("Organizer_id")]
        public int Id { get; set; }

        [Column("Organizer_Name"), Required, MaxLength(150)]
        public string Name { get; set; } = "";

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("Organization_Name"), MaxLength(150)]
        public string? OrganisationName { get; set; }

        [Column("Password_Hash"), Required]
        public string PasswordHash { get; set; } = "";   // NEW: hash only, never the real password

        public List<Event> Events { get; set; } = new(); 
    }

}
