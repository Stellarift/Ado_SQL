using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MailingListApp; 

namespace MailingListApp.Tests
{
    public class MailingTests
    {
        [Fact]
        public void Test_PopularSection_Calculation()
        {
            // Настраиваем тестовую БД в памяти
            var options = new DbContextOptionsBuilder<MailingContext>()
                .UseInMemoryDatabase(databaseName: "TestMailingDb")
                .Options;

            using var db = new MailingContext(options);
            
            // Добавляем тестовые данные
            var s1 = new Section { Name = "Спорт" };
            var s2 = new Section { Name = "Мода" };
            db.Sections.AddRange(s1, s2);
            db.SaveChanges();

            db.Customers.AddRange(
                new Customer { Name = "A", Gender = "M", SectionId = s1.Id },
                new Customer { Name = "B", Gender = "M", SectionId = s1.Id },
                new Customer { Name = "C", Gender = "F", SectionId = s2.Id }
            );
            db.SaveChanges();

            // Проверяем логику поиска популярного раздела
            var popular = db.Customers.GroupBy(c => c.Section!.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).First();

            // Утверждаем, что Спорт выиграл 
            Assert.Equal("Спорт", popular.Name);
            Assert.Equal(2, popular.Count);
        }
    }
}