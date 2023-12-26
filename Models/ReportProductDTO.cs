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
        public string? namaLengkap { get; set; }

        [Column("Nomor_Telepon")]
        public string? nomorTelp { get; set; }

        [Column("Email")]
        public string? email { get; set; }
        [Column("Nama_Produk")]
        public string? namaProduk { get; set; }
        [Column("Deskripsi_Pelapor")]
        public string? descPelapor { get; set; }

        [Column("Create_Date")]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Default value set to current UTC time


        public string? latitude { get; set; }
        public string? longitude { get; set; }

        [Column("Deskripsi_Laporan")]
        public string? descLaporan { get; set; }

        [Column("alamat_map")]
        public string? alamatMap { get; set; }
        [Column("Kelurahan")]
        public string? kelurahan { get; set; }
        [Column("Kecamatan")]
        public string? kecamatan { get; set; }
        [Column("Kabupaten")]
        public string? kabupaten { get; set; }
        [Column("Provinsi")]
        public string? provinsi { get; set; }
        [Column("Negara")]
        public string? negara { get; set; }

        [NotMapped] // This property will not be mapped to the database
        public string? gRecaptchaResponse { get; set; }
        [NotMapped]
        public string? gRecaptchasitekey { get; set; }
    }

    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        public string ChallengeTs { get; set; }
        public string Hostname { get; set; }
        public decimal Score { get; set; }
        public string Action { get; set; }
        public IList<string> ErrorCodes { get; set; }
    }
}
