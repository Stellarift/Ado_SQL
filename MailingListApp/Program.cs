using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MailingListApp
{
    // модели данных
    public class Section { public int Id { get; set; } public string Name { get; set; } = ""; }
    public class Customer { public int Id { get; set; } public string Name { get; set; } = ""; public string Gender { get; set; } = ""; public int Age { get; set; } public string City { get; set; } = ""; public string Country { get; set; } = ""; public int SectionId { get; set; } public Section? Section { get; set; } }
    public class Product { public int Id { get; set; } public string Name { get; set; } = ""; public int SectionId { get; set; } public bool IsPromotional { get; set; } public DateTime? PromoEndDate { get; set; } }
    public class PromoArchive { public int Id { get; set; } public int ProductId { get; set; } public string Name { get; set; } = ""; public DateTime ArchivedAt { get; set; } }

    // контекст
        public class MailingContext : DbContext
    {
        public DbSet<Section> Sections { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PromoArchive> PromoArchives { get; set; }

        // 1. Пустой конструктор (для основного приложения)
        public MailingContext() { }

        // 2. Конструктор с настройками (для тестов)
        public MailingContext(DbContextOptions<MailingContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer("Server=localhost,1433;Database=MailingDb;User Id=sa;Password=Ghoul1234;TrustServerCertificate=True;");
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            using (var db = new MailingContext())
            {
                Console.WriteLine("2) Популярность разделов ---");
                var popSections = db.Customers.GroupBy(c => c.Section!.Name).Select(g => new { Name = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToList();
                
                Console.WriteLine("\nТоп-3 популярных:");
                foreach (var s in popSections.Take(3)) Console.WriteLine($"{s.Name} — {s.Count} чел.");
                Console.WriteLine($"Самый популярный: {popSections.First().Name}");

                Console.WriteLine("\nТоп-3 непопулярных:");
                foreach (var s in popSections.OrderBy(x => x.Count).Take(3)) Console.WriteLine($"{s.Name} — {s.Count} чел.");
                Console.WriteLine($"Самый непопулярный: {popSections.OrderBy(x => x.Count).First().Name}");


                Console.WriteLine("\n3) Акционные товары по разделам ---");
                var promoStats = db.Products.Where(p => p.IsPromotional).GroupBy(p => p.SectionId).Select(g => new { SectionId = g.Key, Count = g.Count() }).ToList();
                // Соединяем с названиями разделов
                var sectionPromo = db.Sections.Select(s => new { s.Name, Count = db.Products.Count(p => p.SectionId == s.Id && p.IsPromotional) }).OrderByDescending(x => x.Count).ToList();

                Console.WriteLine("\nТоп-3 по акционным товарам:");
                foreach (var s in sectionPromo.Take(3)) Console.WriteLine($"{s.Name} — {s.Count} шт.");
                Console.WriteLine($"Больше всего акций: {sectionPromo.First().Name}");

                Console.WriteLine("\nТоп-3 с наименьшим количеством акций:");
                foreach (var s in sectionPromo.OrderBy(x => x.Count).Take(3)) Console.WriteLine($"{s.Name} — {s.Count} шт.");
                Console.WriteLine($"Меньше всего акций: {sectionPromo.OrderBy(x => x.Count).First().Name}");


                Console.WriteLine("\n4) Работа с акциями ---");
                var endIn3Days = db.Products.Where(p => p.IsPromotional && p.PromoEndDate <= DateTime.Now.AddDays(3) && p.PromoEndDate > DateTime.Now).ToList();
                Console.WriteLine("\nАкции, заканчивающиеся через 3 дня:");
                foreach (var p in endIn3Days) Console.WriteLine($"{p.Name} (до {p.PromoEndDate:dd.MM.yyyy})");

                var expired = db.Products.Where(p => p.IsPromotional && p.PromoEndDate < DateTime.Now).ToList();
                Console.WriteLine("\nПросроченные акции:");
                foreach (var p in expired) Console.WriteLine(p.Name);

                // Перенос в архив
                if (expired.Any())
                {
                    foreach (var p in expired)
                    {
                        db.PromoArchives.Add(new PromoArchive { ProductId = p.Id, Name = p.Name, ArchivedAt = DateTime.Now });
                        p.IsPromotional = false; // Снимаем статус
                        p.PromoEndDate = null;
                    }
                    db.SaveChanges();
                    Console.WriteLine("Просроченные акции перенесены в Архив.");
                }

                Console.WriteLine("\nСредний возраст покупателей по разделам:");
                var avgBySection = db.Customers.GroupBy(c => c.Section!.Name).Select(g => new { Name = g.Key, AvgAge = g.Average(c => c.Age) });
                foreach (var s in avgBySection) Console.WriteLine($"{s.Name}: {s.AvgAge:F1} лет");

                Console.WriteLine("\nСредний возраст по городам:");
                var avgByCity = db.Customers.GroupBy(c => c.City).Select(g => new { City = g.Key, AvgAge = g.Average(c => c.Age) });
                foreach (var c in avgByCity) Console.WriteLine($"{c.City}: {c.AvgAge:F1} лет");

                Console.WriteLine("\nСредний возраст по странам:");
                var avgByCountry = db.Customers.GroupBy(c => c.Country).Select(g => new { Country = g.Key, AvgAge = g.Average(c => c.Age) });
                foreach (var c in avgByCountry) Console.WriteLine($"{c.Country}: {c.AvgAge:F1} лет");


                Console.WriteLine("\n5)Анализ по полу ---");
                Console.WriteLine("\nСамый популярный раздел для каждого пола:");
                
                // Добавляем проверку на null (x.Section != null)
                var popByGender = db.Customers.GroupBy(c => c.Gender).Select(g => new {
                    Gender = g.Key,
                    TopSection = g.Where(x => x.Section != null)
                                  .GroupBy(x => x.Section!.Name)
                                  .OrderByDescending(x => x.Count())
                                  .FirstOrDefault()!.Key
                }).ToList();
                
                foreach (var item in popByGender) Console.WriteLine($"{item.Gender}: {item.TopSection}");

                Console.WriteLine("\nТоп-3 разделов для каждого пола:");
                var genders = db.Customers.Select(c => c.Gender).Distinct().ToList();
                foreach (var gender in genders)
                {
                    Console.WriteLine($"[{gender}]");
                    var top3 = db.Customers.Where(c => c.Gender == gender && c.Section != null)
                        .GroupBy(c => c.Section!.Name)
                        .Select(g => new { Name = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(3)
                        .ToList();
                    foreach (var s in top3) Console.WriteLine($"  {s.Name} — {s.Count}");
                }

                Console.WriteLine("\nКоличество покупателей каждого пола:");
                var countByGender = db.Customers.GroupBy(c => c.Gender).Select(g => new { Gender = g.Key, Count = g.Count() });
                foreach (var c in countByGender) Console.WriteLine($"{c.Gender}: {c.Count}");

                Console.WriteLine("\nКоличество покупателей каждого пола по странам:");
                var countByGenderCountry = db.Customers.GroupBy(c => new { c.Country, c.Gender }).Select(g => new { g.Key.Country, g.Key.Gender, Count = g.Count() });
                foreach (var c in countByGenderCountry) Console.WriteLine($"{c.Country} | {c.Gender}: {c.Count}");
            }
        }
    }
}
