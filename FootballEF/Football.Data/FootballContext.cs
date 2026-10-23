using Microsoft.EntityFrameworkCore;
using Football.Domain;

namespace Football.Data
{
    public class FootballContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }

        // 1. Пустой конструктор
        public FootballContext() { }

        // 2. Конструктор с настройками
        public FootballContext(DbContextOptions<FootballContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Настраиваем SQL Server только если настройки еще не переданы
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost,1433;Database=FootballDb;User Id=sa;Password=Ghoul1234;TrustServerCertificate=True;");
            }
        }

        // Настройка связей
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}