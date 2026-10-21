using Microsoft.EntityFrameworkCore;

namespace CarRentalDbFirst
{
    public class CarRentalContext : DbContext
    {
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Car> Cars { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=CarRentalDb;User Id=sa;Password=Ghoul1234;TrustServerCertificate=True;");
        }
    }
}