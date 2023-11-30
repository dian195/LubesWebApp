using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Repository
{
    public class AppDB : DbContext
    {
        public AppDB(DbContextOptions<AppDB> options) : base(options)
        {

        }

        public DbSet<SeriesMasterDTO> series_master { get; set; }
        public DbSet<LogScanningDTO> log_scanning { get; set; }
        public DbSet<ReportProductDTO> report_product { get; set; }
        public DbSet<UserDTO> account { get; set; }
        public DbSet<RoleDTO> role { get; set; }
    }
}
