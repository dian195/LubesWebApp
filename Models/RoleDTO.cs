using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    [Table("role_user")]
    public class RoleDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name: "id")]
        public int Id { get; set; }
        [Column(name: "Role_Name")]
        public required string Role_Name { get; set; }
        [Column(name: "IsActive")]
        public int IsActive { get; set; }
    }
}
