using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Messenjoor.Entities
{
    [Table("User")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, Unicode(true), MaxLength(25)]
        public required string Name { get; set; }

        public DateTime AddedOn { get; set; }

        [Required, Unicode(false), MaxLength(50)]
        public required string Username { get; set; }

        [Required, MaxLength(20)]
        public required string Password { get; set; }


    }
}
