using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACE_DAY_.Models
{

    [Table("Participant")]
    public class Participant
    {
        [Key, Column("Participant_id")]
        public int Id { get; set; }

        [Column("First_Name"), Required, MaxLength(150)]
        public string FirstName { get; set; } = "";

        [Column("Last_Name"), Required, MaxLength(150)]
        public string LastName { get; set; } = "";

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [Column("Cell"), MaxLength(20)]
        public string? Phone { get; set; }

        [Column("DoB")]
        public DateOnly Dob { get; set; } // add this info to read me

        [Column("Password_Hash"), Required]
        public string PasswordHash { get; set; } = "";   // NEW
    }

}
