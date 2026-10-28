using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MailingDapperApp
{
    // Модели
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = "";
        public string Email { get; set; } = "";
        public string Country { get; set; } = "";
        public string City { get; set; } = "";
    }

    public class PromoProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // DTO для аналитики
    public class CountDto { public string Name { get; set; } = ""; public int Count { get; set; } }

    class Program
    {
        static string connectionString = "Server=localhost,1433;Database=MailingDapperDb;User Id=sa;Password=Ghoul1234;TrustServerCertificate=True;";

        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Задание 2: Проверка подключения
            try
            {
                using IDbConnection db = new SqlConnection(connectionString);
                db.Open();
                Console.WriteLine("Успешное подключение к базе данных 'Список рассылки'!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                return;
            }

            while (true)
            {
                Console.WriteLine("\n--- МЕНЮ (Dapper Часть 2) ---");
                Console.WriteLine("1-6. Базовые запросы (из Части 1)");
                Console.WriteLine("7-9. Фильтры по городу/стране (из Части 1)");
                Console.WriteLine("10. Кол-во покупателей в каждом городе");
                Console.WriteLine("11. Кол-во покупателей в каждой стране");
                Console.WriteLine("12. Кол-во городов в каждой стране");
                Console.WriteLine("13. Среднее кол-во городов по странам");
                Console.WriteLine("14. Разделы для покупателей конкретной страны");
                Console.WriteLine("15. Акции раздела за промежуток времени");
                Console.WriteLine("16. Акции для конкретного покупателя");
                Console.WriteLine("17. Топ-3 стран по покупателям");
                Console.WriteLine("18. Лучшая страна по покупателям");
                Console.WriteLine("19. Топ-3 городов по покупателям");
                Console.WriteLine("20. Лучший город по покупателям");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine() ?? "";
                if (choice == "0") break;

                try
                {
                    using var db = new SqlConnection(connectionString);
                    await db.OpenAsync(); // Асинхронное открытие
                    
                    switch (choice)
                    {
                        // ЧАСТЬ 1 (Базовые)
                        case "1":
                            var customers = await db.QueryAsync<Customer>("SELECT * FROM Customers");
                            foreach (var c in customers) Console.WriteLine($"{c.Id}. {c.FullName} | {c.Country}, {c.City}");
                            break;
                        case "7":
                            Console.Write("Город: "); string city = Console.ReadLine() ?? "";
                            var cityCust = await db.QueryAsync<Customer>("SELECT * FROM Customers WHERE City = @City", new { City = city });
                            foreach (var c in cityCust) Console.WriteLine($"{c.FullName} ({c.Email})");
                            break;

                        // ЗАДАНИЕ 1
                        case "10":
                            var cityCounts = await db.QueryAsync<CountDto>("SELECT City AS Name, COUNT(*) AS Count FROM Customers GROUP BY City");
                            Console.WriteLine("\nПокупателей в каждом городе:");
                            foreach (var item in cityCounts) Console.WriteLine($"{item.Name}: {item.Count}");
                            break;

                        case "11":
                            var countryCounts = await db.QueryAsync<CountDto>("SELECT Country AS Name, COUNT(*) AS Count FROM Customers GROUP BY Country");
                            Console.WriteLine("\nПокупателей в каждой стране:");
                            foreach (var item in countryCounts) Console.WriteLine($"{item.Name}: {item.Count}");
                            break;

                        case "12":
                            var citiesInCountry = await db.QueryAsync<CountDto>("SELECT Country AS Name, COUNT(DISTINCT City) AS Count FROM Customers GROUP BY Country");
                            Console.WriteLine("\nГородов в каждой стране:");
                            foreach (var item in citiesInCountry) Console.WriteLine($"{item.Name}: {item.Count}");
                            break;

                        case "13":
                            var avgCities = await db.QuerySingleAsync<double>("SELECT AVG(Cast(CityCount as Float)) FROM (SELECT COUNT(DISTINCT City) as CityCount FROM Customers GROUP BY Country) as SubQuery");
                            Console.WriteLine($"\nСреднее количество городов по всем странам: {avgCities:F2}");
                            break;

                        // ЗАДАНИЕ 2: 
                        case "14":
                            Console.Write("Введите страну: "); string targetCountry = Console.ReadLine() ?? "";
                            string sqlSections = @"
                                SELECT DISTINCT s.Name 
                                FROM Sections s
                                JOIN CustomerSections cs ON cs.SectionId = s.Id
                                JOIN Customers c ON c.Id = cs.CustomerId
                                WHERE c.Country = @Country";
                            var sections = await db.QueryAsync<string>(sqlSections, new { Country = targetCountry });
                            Console.WriteLine($"\nРазделы, интересующие покупателей из {targetCountry}:");
                            foreach (var s in sections) Console.WriteLine($"- {s}");
                            break;

                        case "15":
                            Console.Write("ID раздела: "); int sectionId = int.Parse(Console.ReadLine() ?? "0");
                            Console.Write("Дата начала (гггг-мм-дд): "); DateTime start = DateTime.Parse(Console.ReadLine() ?? "2026-01-01");
                            Console.Write("Дата конца (гггг-мм-дд): "); DateTime end = DateTime.Parse(Console.ReadLine() ?? "2026-12-31");
                            var promosInRange = await db.QueryAsync<PromoProduct>(
                                "SELECT * FROM PromoProducts WHERE SectionId = @SectionId AND StartDate >= @Start AND EndDate <= @End", 
                                new { SectionId = sectionId, Start = start, End = end });
                            Console.WriteLine("\nАкции за указанный период:");
                            foreach (var p in promosInRange) Console.WriteLine($"{p.Name} (до {p.EndDate:dd.MM.yyyy})");
                            break;

                        case "16":
                            Console.Write("ID покупателя: "); int custId = int.Parse(Console.ReadLine() ?? "0");
                            string sqlCustomerPromos = @"
                                SELECT DISTINCT p.* 
                                FROM PromoProducts p
                                JOIN CustomerSections cs ON cs.SectionId = p.SectionId
                                JOIN Customers c ON c.Id = cs.CustomerId
                                WHERE c.Id = @CustomerId AND p.Country = c.Country";
                            var custPromos = await db.QueryAsync<PromoProduct>(sqlCustomerPromos, new { CustomerId = custId });
                            Console.WriteLine("\nАкции, доступные этому покупателю (в его стране и его разделах):");
                            foreach (var p in custPromos) Console.WriteLine($"{p.Name} (до {p.EndDate:dd.MM.yyyy})");
                            break;

                        // ЗАДАНИЕ 3:
                        case "17":
                            var top3Countries = await db.QueryAsync<CountDto>("SELECT TOP 3 Country AS Name, COUNT(*) AS Count FROM Customers GROUP BY Country ORDER BY Count DESC");
                            Console.WriteLine("\nТоп-3 стран по количеству покупателей:");
                            foreach (var item in top3Countries) Console.WriteLine($"{item.Name}: {item.Count}");
                            break;

                        case "18":
                            var bestCountry = await db.QuerySingleAsync<CountDto>("SELECT TOP 1 Country AS Name, COUNT(*) AS Count FROM Customers GROUP BY Country ORDER BY Count DESC");
                            Console.WriteLine($"\nЛучшая страна: {bestCountry.Name} ({bestCountry.Count} чел.)");
                            break;

                        case "19":
                            var top3Cities = await db.QueryAsync<CountDto>("SELECT TOP 3 City AS Name, COUNT(*) AS Count FROM Customers GROUP BY City ORDER BY Count DESC");
                            Console.WriteLine("\nТоп-3 городов по количеству покупателей:");
                            foreach (var item in top3Cities) Console.WriteLine($"{item.Name}: {item.Count}");
                            break;

                        case "20":
                            var bestCity = await db.QuerySingleAsync<CountDto>("SELECT TOP 1 City AS Name, COUNT(*) AS Count FROM Customers GROUP BY City ORDER BY Count DESC");
                            Console.WriteLine($"\nЛучший город: {bestCity.Name} ({bestCity.Count} чел.)");
                            break;

                        default:
                            Console.WriteLine("Нет такого пункта (или он не реализован).");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}
