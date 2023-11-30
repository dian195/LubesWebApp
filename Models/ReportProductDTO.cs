using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    [Table("report_product")]
    public class ReportProductDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Nama_Lengkap")]
        public string namaLengkap { get; set; }

        [Column("Nomor_Telepon")]
        public string nomorTelp { get; set; }

        [Column("Email")]
        public string email { get; set; }
        [Column("Nama_Produk")]
        public string namaProduk { get; set; }
        [Column("Deskripsi_Pelapor")]
        public string descPelapor { get; set; }

        [Column("Create_Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default value set to current UTC time
    }
}
