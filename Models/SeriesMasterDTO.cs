using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class SeriesMasterDTO
    {
        public SeriesMasterDTO()
        {
        }

        [Key]
        public int? id { get; set; }
        [Required]
        [Column(name: "series_id")]
        public string? seriesID { get; set; }
        [Required]
        [Column(name: "product_name")]
        public string? productName { get; set; }
        [Required]
        [Column(name: "product_packaging")]
        public string? productPackaging { get; set; }
        [Column(name: "product_volume")]
        public string? productVolume { get; set; }
        [Column("Created_By")]
        public string? createdBy { get; set; }
        [Column("Created_Date")]
        public DateTime? createdDate { get; set; }
        [Column("Last_Update_By")]
        public string? lastUpdateBy { get; set; }
        [Column("Last_Update")]
        public DateTime? lastUpdate { get; set; }
        [Column(name: "no_kimap")]
        public string? noKimap { get; set; }
    }

    public class AddSeriesMasterDTO
    {
        [Required]
        public string? seriesId { get; set; }
        [Required]
        public string? noKimap { get; set; }
        [Required]
        public string? productName { get; set; }
        [Required]
        public string? productPackaging { get; set; }
        [Required]
        public string? productVolume { get; set; }
    }

    public class EditSeriesMasterDTO
    {
        [Required]
        public int? id { get; set; }
        [Required]
        public string? seriesId { get; set; }
        [Required]
        public string? noKimap { get; set; }
        [Required]
        public string? productName { get; set; }
        [Required]
        public string? productPackaging { get; set; }
        [Required]
        public string? productVolume { get; set; }
    }

}
