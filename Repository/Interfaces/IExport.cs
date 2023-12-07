using WebApp.Models;

namespace WebApp.Repository.Interfaces
{
    public interface IExport
    {
        void exportPengaduan(string fromDate, string toDate, string filter, string kota, string prov, List<ReportProductDTO> dataExport, ref MemoryStream mem);
        void exportProduct(string filter, List<SeriesMasterDTO> dataExport, ref MemoryStream mem);
        void exportScan(string filter, string from, string to, string kota, string prov, List<LogScanningDTO> dataExport, ref MemoryStream mem);

    }
}
