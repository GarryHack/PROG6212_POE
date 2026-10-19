using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACE_DAY_.Models
{
    [Table("Registration")]   // = an "enrolment" in the API
    public class Registration
    {
        [Key, Column("Registration_id")]
        public int Id { get; set; }

        [Column("Participant_id")]
        public int ParticipantId { get; set; }
        public Participant? Participant { get; set; }

        [Column("Category_id")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Column("Registration_date")]
        public DateOnly RegistrationDate { get; set; }

        [Column("Payment_status"), MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending";
    }
}
