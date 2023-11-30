using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace WebApp.Models
{
    [Table("accounts")]
    public class UserDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name: "UserId")]
        public int? userId { get; set; }
        [Required]
        [Column(name: "Username")]
        public string? username { get; set; }
        [Required]
        [Column(name: "Nama_User")]
        public string? namaUser { get; set; }
        [Required]
        [Column(name: "RoleID")] //// Foreign key property to the Department
        public int roleId { get; set; }
        [Required]
        [Column(name: "Password")]
        public string? password { get; set; }
        [Required]
        [Column(name: "Email")]
        public string? email { get; set; }
        [Column("Last_Login")]
        public DateTime? lastLogin { get; set; }
        [Column("IsActive")]
        public int isActive { get; set; }
        [Column("Created_By")]
        public string? createdBy { get; set; }
        [Column("Created_Date")]
        public DateTime? createdDate { get; set; }
        [Column("Last_Update_By")]
        public string? lastUpdateBy { get; set; }
        [Column("Last_Update")]
        public DateTime? lastUpdate { get; set; }


        // Reference navigation to tabel role
        public RoleDTO Role { get; set; } = null!;
    }

    public class UserProfileDTO
    {
        public int? userId { get; set; }
        public string? username { get; set; }
        public string? namaUser { get; set; }
        public int roleId { get; set; }
        public string? rolename { get; set; }
        public string? password { get; set; }
        public string? email { get; set; }
        public List<RoleDTO> Roles { get; set; } = null!;
    }

    public class UpdatePassProfileDTO
    {
        public int? usrId { get; set; }
        public string? oldpass { get; set; }
        public string? newpass { get; set; }
        public string? confirmpass { get; set; }
    }

    public class UserGVDTO
    {
        public StaticPagedList<UserDTO> Users { get; set; }
        public string? filter { get; set; }
        public int? pg;
    }
}
