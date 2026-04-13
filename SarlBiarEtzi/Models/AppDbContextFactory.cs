using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SarlBiarEtzi.Models
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseNpgsql(
                "Host=caboose.proxy.rlwy.net;Port=59533;Database=railway;Username=postgres;Password=UycXfewxLBYrPGFhOCIOSTLWolQaKcIL"
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
