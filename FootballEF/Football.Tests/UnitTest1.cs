using System.Linq;
using Microsoft.EntityFrameworkCore;
using Football.Data;
using Football.Domain;
using Xunit;

namespace Football.Tests
{
    public class FootballTests
    {
        private FootballContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<FootballContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            return new FootballContext(options);
        }

        [Fact]
        public void Test_TeamWithMostGoals()
        {
            using var db = GetInMemoryContext();
            // Добавляем данные
            var t1 = new Team { Name = "A" };
            var t2 = new Team { Name = "B" };
            db.Teams.AddRange(t1, t2);
            db.SaveChanges();

            db.Matches.Add(new Match { HomeTeamId = t1.Id, AwayTeamId = t2.Id, HomeGoals = 5, AwayGoals = 1 });
            db.SaveChanges();

            // Проверяем логику
            var totalGoalsT1 = db.Matches.Where(m => m.HomeTeamId == t1.Id).Sum(m => m.HomeGoals) +
                               db.Matches.Where(m => m.AwayTeamId == t1.Id).Sum(m => m.AwayGoals);
                               
            Assert.Equal(5, totalGoalsT1);
        }

        [Fact]
        public void Test_PlayerGoals()
        {
            using var db = GetInMemoryContext();
            var player = new Player { Name = "Messi", Goals = 30 };
            db.Players.Add(player);
            db.SaveChanges();

            var topPlayer = db.Players.OrderByDescending(p => p.Goals).First();
            Assert.Equal("Messi", topPlayer.Name);
            Assert.Equal(30, topPlayer.Goals);
        }
    }
}