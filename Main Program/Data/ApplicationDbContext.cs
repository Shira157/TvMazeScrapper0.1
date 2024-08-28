using Microsoft.EntityFrameworkCore;
using TvMazeScrapper.Models;

namespace TvMazeScrapper.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Show> Shows { get; set; }
        public DbSet<CastPerson> CastPersons { get; set; }
        public DbSet<ShowMetaData> ShowMetaData { get; set; }
    }
}
