namespace WebApp.Models
{
    public class DashboardDTO
    {
        public decimal? totalScan { get; set; }
        public decimal? totalPengaduan { get; set; }
        public decimal? totalProduct { get; set; }

        public List<LogScanningDTO> logScanning { get; set; }
        public List<ReportProductDTO> reportProduct { get; set; }
    }
}
