using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using WebApp.Models;
using WebApp.Repository;
using WebApp.Repository.Interfaces;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDB _context;
        private readonly IConfiguration _appconf;
        private readonly IAPIQRScanServices _API;

        public HomeController(ILogger<HomeController> logger, AppDB context, IAPIQRScanServices api, IConfiguration appconf)
        {
            _logger = logger;
            _context = context;
            _API = api;
            _appconf = appconf;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("~/Report")]
        public IActionResult Report()
        {
            return View();
        }        

        [HttpGet("/{param1}/{param2}")]
        public async Task<IActionResult> Index(string param1, string param2)
        {
            var prodDetail = new ProductDetailDTO();
            SeriesMasterDTO? series = null;
            series = new SeriesMasterDTO();
            series = _context.series_master.FirstOrDefault(s => s.seriesID == param1);
            ViewBag.uniqueNumber = param2;

            ApiResponseDTO response = await _API.GetURLApi(param2, "qZL$HhAStAxtcVax%4%)KEhGjH9$fB", _appconf.GetConnectionString("URLAdvo"));
            ApiResponseDTO response2 = await _API.MakeApiRequest(_appconf.GetConnectionString("APIURL") + param2);
            Console.WriteLine(response2.IsSuccess);

            if (response.IsSuccess == 200) //PDM
            {
                dynamic product = JsonConvert.DeserializeObject(response.Data);

                ViewBag.ApiResponse = product;
                prodDetail.qrCode = param2;
                prodDetail.productCode = product.productCode;
                prodDetail.productName = product.productName;
                //prodDetail.productUnit = product.productUnit;
                prodDetail.productPackaging = product.productUnit;
                prodDetail.productionBatch = product.productionBatch == null ? "-" : (product.productionBatch == "" ? "-" : product.productionBatch);

                string unit = product.productUnit;

                //Get Last Scan
                var lastscan = _API.GetLastScan(param2, _context);
                if (lastscan != null)
                {
                    prodDetail.lastAlamatMap = lastscan.Count() > 0 ? (lastscan.OrderByDescending(s => s.CreatedAt).First().alamatMap ?? "-") : "-";
                    prodDetail.lastScanTimestamp = lastscan.Count() > 0 ? (lastscan.OrderByDescending(s => s.CreatedAt).First().CreatedAt.ToString("dd/MM/yyyy HH:mm:ss") ?? "-") : "-";
                    prodDetail.jmlScan = lastscan.Count() + 1;
                }

                if (unit.Trim() == "DR" || unit.Trim() == "DRUM")
                {
                    return View("Index_Drum", prodDetail); //PDM
                }
                if (unit.Trim() == "BOX" || unit.Trim() == "LITHOS")
                {
                    return View("Index_Lithos", prodDetail); //PDM
                }
            }
            else
            {
                if (response2.IsSuccess == 200)
                {
                    Console.WriteLine(series);
                    if (series == null)
                    {
                        return View("NotFound");
                    }
                    else
                    {
                        prodDetail.qrCode = param2;
                        //prodDetail.productCode = series.productCode;
                        prodDetail.productName = series.productName;
                        prodDetail.seriesId = series.seriesID;
                        prodDetail.productPackaging = series.productPackaging;
                        //prodDetail.productionBatch = series.productionBatch;
                        prodDetail.productionBatch = "-";

                        //Get Last Scan
                        var lastscan = _API.GetLastScan(param2, _context);
                        if (lastscan != null)
                        {
                            prodDetail.lastAlamatMap = lastscan.Count() > 0 ? (lastscan.OrderByDescending(s => s.CreatedAt).First().alamatMap ?? "-") : "-";
                            prodDetail.lastScanTimestamp = lastscan.Count() > 0 ? (lastscan.OrderByDescending(s => s.CreatedAt).First().CreatedAt.ToString("dd/MM/yyyy HH:mm:ss") ?? "-") : "-";
                            prodDetail.jmlScan = lastscan.Count() + 1;
                        }

                        if (series.productPackaging == "DRUM" || series.productPackaging == "DR")
                        {
                            return View("Index_Drum", prodDetail);
                        }

                        return View("Index_Lithos", prodDetail);
                    }
                }
                else
                {
                    return View("NotFound");
                }
            }

            return View("NotFound");
        }

        [Route("~/Login")]
        public IActionResult Login()
        {
            return View();
        }

        [Route("~/Error")]
        public IActionResult ErrorLocation()
        {
            return View("ErrorLocation");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}