namespace WebApp.Models
{
    public class ApiResponseDTO
    {
        public int? IsSuccess { get; set; }
        public string? Message { get; set; }

        // Optional: You can include additional data in the response
        public string? Data { get; set; }
    }

    public class ProductDetailDTO
    {
        public string? qrCode { get; set; }
        public string? seriesId { get; set; }
        public string? productPackaging { get; set; }
        public string? productVolume { get; set; }
        public string? productionBatch { get; set; }
        public string? productUnit { get; set; }
        public string? productCode { get; set; }
        public string? productName { get; set; }
        public int jmlScan { get; set; }
        public string? lastLongitude { get; set; }
        public string? lastLatitude { get; set; }
        public string? lastAlamatMap { get; set; }
    }
}
