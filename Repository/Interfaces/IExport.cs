using WebApp.Models;

namespace WebApp.Repository.Interfaces
{
    public interface IExport
    {
        void exportPengaduan(string fromDate, string toDate, string filter, List<ReportProductDTO> dataExport, ref MemoryStream mem);
        void exportProduct(string filter, List<SeriesMasterDTO> dataExport, ref MemoryStream mem);
        void exportScan(string filter, List<LogScanningDTO> dataExport, ref MemoryStream mem);

    }
}
