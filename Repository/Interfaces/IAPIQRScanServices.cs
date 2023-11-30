using WebApp.Models;

namespace WebApp.Repository.Interfaces
{
    public interface IAPIQRScanServices
    {
        Task<ApiResponseDTO> GetURLApi(string serialNumber, string APP_SECRET, string urlAdvo);
        Task<ApiResponseDTO> MakeApiRequest(string apiUrl);
        IQueryable<LogScanningDTO> GetLastScan(string qrNo, AppDB dbContext);
    }
}
