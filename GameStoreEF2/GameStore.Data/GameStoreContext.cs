using Microsoft.EntityFrameworkCore;
using GameStore.Domain;

namespace GameStore.Data
{
    public class GameStoreContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost,1433;Database=GameStoreDb;User Id=sa;Password=Ghoul1234;TrustServerCertificate=True;");
            }
        }

        public DbSet<Game> Games { get; set; }
    }
}