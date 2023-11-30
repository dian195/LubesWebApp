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
    }
}
