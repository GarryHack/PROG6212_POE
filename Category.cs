using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RACE_DAY_.Models
{
    [Table("Category")]
    public class Category
    {
        [Key, Column("Category_id")]
        public int Id { get; set; }

        [Column("Event_id")]
        public int EventId { get; set; }                  // foreign key column
        public Event? Event { get; set; }

        [Column("Category_Name"), Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }

    }

}
