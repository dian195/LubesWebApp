using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Repository.Interfaces;

namespace WebApp.Repository.Services
{
    public class APIQRScanServices : IAPIQRScanServices
    {
        private GlobalFunction gf = new GlobalFunction();

        // Tarik Data Dari PDM ( balikan data nya json )
        public async Task<ApiResponseDTO> GetURLApi(string serialNumber, string APP_SECRET, string urlAdvo)
        {
            Dictionary<string, string> paramMap = new Dictionary<string, string>
            {
                { "serialNumber", serialNumber },
            };

            try
            {
                urlAdvo += gf.JoinParams(paramMap, APP_SECRET);
                Console.WriteLine(urlAdvo);
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, urlAdvo);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(response);
                string data = await response.Content.ReadAsStringAsync();

                return new ApiResponseDTO { IsSuccess = 200, Message = "Operation successful", Data = data };

            }
            catch (Exception e)
            {
                return new ApiResponseDTO { IsSuccess = 400, Message = "Nomor QR Tidak Ditemukan " };
            }
        }

        // Tarik Data Dari QRService (balikan datanya JSON )
        public async Task<ApiResponseDTO> MakeApiRequest(string apiUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                    string contents = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        return new ApiResponseDTO { IsSuccess = 200, Message = "Operation successful" };
                    }
                    else
                    {
                        return new ApiResponseDTO { IsSuccess = 400, Message = "Nomor QR Tidak Ditemukan " };
                    }
                }
                catch (Exception ex)
                {
                    return new ApiResponseDTO { IsSuccess = 400, Message = "Nomor QR Tidak Ditemukan " };
                }
            }
        }

        public IQueryable<LogScanningDTO> GetLastScan(string qrNo, AppDB dbContext)
        {
            var log = dbContext.log_scanning.Where(s => s.qrNo == qrNo);
            return log;
        }


    }
}
