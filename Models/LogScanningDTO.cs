using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    [Table("log_scanning")]
    public class LogScanningDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("qr_no")]
        public string? qrNo { get; set; }

        [Column("product_id")]
        public string? productId { get; set; }

        [Column("customer_profile")]
        public string? customerProfile { get; set; }
        public string? latitude { get; set; }
        public string? longitude { get; set; }

        [Column("alamat_map")] 

        public string? alamatMap { get; set; }

        [Column("created_at")] 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default value set to current UTC time
    }
}
