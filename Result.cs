using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACE_DAY_.Models
{
    [Table("Result")]
    public class Result
    {
        [Key, Column("Result_id")]
        public int Id { get; set; }

        [Column("Registration_id")]
        public int RegistrationId { get; set; }          
        public Registration? Registration { get; set; }

        [Column("Finish_Time")]
        public TimeSpan FinishTime { get; set; }

        public int Position { get; set; }

        [Column("Weather_Info"), MaxLength(200)]
        public string? WeatherInfo { get; set; }
    }

}
