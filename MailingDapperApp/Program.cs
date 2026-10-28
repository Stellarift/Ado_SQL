using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MailingDapperApp
{
    // Модели для Dapper
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

    class Program
    {
        static string connectionString = "Server=localhost,1433;Database=MailingDapperDb;User Id=sa;Password=Ghoul1234;TrustServerCertificate=True;";

        static void Main()
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
                Console.WriteLine("\nменю");
                Console.WriteLine("1. Все покупатели");
                Console.WriteLine("2. Email всех покупателей");
                Console.WriteLine("3. Список разделов");
                Console.WriteLine("4. Список акционных товаров");
                Console.WriteLine("5. Все города");
                Console.WriteLine("6. Все страны");
                Console.WriteLine("7. Покупатели из конкретного города");
                Console.WriteLine("8. Покупатели из конкретной страны");
                Console.WriteLine("9. Акции для конкретной страны");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine() ?? "";
                if (choice == "0") break;

                try
                {
                    using IDbConnection db = new SqlConnection(connectionString);
                    
                    switch (choice)
                    {
                        case "1":
                            var customers = db.Query<Customer>("SELECT * FROM Customers").ToList();
                            Console.WriteLine("\nВсе покупатели:");
                            foreach (var c in customers)
                                Console.WriteLine($"{c.Id}. {c.FullName} | {c.Email} | {c.Country}, {c.City}");
                            break;

                        case "2":
                            var emails = db.Query<string>("SELECT Email FROM Customers").ToList();
                            Console.WriteLine("\nEmail покупателей:");
                            foreach (var e in emails) Console.WriteLine(e);
                            break;

                        case "3":
                            var sections = db.Query<string>("SELECT Name FROM Sections").ToList();
                            Console.WriteLine("\nРазделы рассылки:");
                            foreach (var s in sections) Console.WriteLine(s);
                            break;

                        case "4":
                            var promos = db.Query<PromoProduct>("SELECT * FROM PromoProducts").ToList();
                            Console.WriteLine("\nАкционные товары:");
                            foreach (var p in promos)
                                Console.WriteLine($"{p.Name} (Страна: {p.Country}, до {p.EndDate:dd.MM.yyyy})");
                            break;

                        case "5":
                            var cities = db.Query<string>("SELECT DISTINCT City FROM Customers").ToList();
                            Console.WriteLine("\nГорода покупателей:");
                            foreach (var c in cities) Console.WriteLine(c);
                            break;

                        case "6":
                            var countries = db.Query<string>("SELECT DISTINCT Country FROM Customers").ToList();
                            Console.WriteLine("\nСтраны покупателей:");
                            foreach (var c in countries) Console.WriteLine(c);
                            break;

                        case "7":
                            Console.Write("Введите город: ");
                            string city = Console.ReadLine() ?? "";
                            var cityCustomers = db.Query<Customer>("SELECT * FROM Customers WHERE City = @City", new { City = city }).ToList();
                            Console.WriteLine($"\nПокупатели из города {city}:");
                            foreach (var c in cityCustomers) Console.WriteLine($"{c.FullName} ({c.Email})");
                            break;

                        case "8":
                            Console.Write("Введите страну: ");
                            string country = Console.ReadLine() ?? "";
                            var countryCustomers = db.Query<Customer>("SELECT * FROM Customers WHERE Country = @Country", new { Country = country }).ToList();
                            Console.WriteLine($"\nПокупатели из страны {country}:");
                            foreach (var c in countryCustomers) Console.WriteLine($"{c.FullName} ({c.City})");
                            break;

                        case "9":
                            Console.Write("Введите страну: ");
                            string promoCountry = Console.ReadLine() ?? "";
                            var countryPromos = db.Query<PromoProduct>("SELECT * FROM PromoProducts WHERE Country = @Country", new { Country = promoCountry }).ToList();
                            Console.WriteLine($"\nАкции для страны {promoCountry}:");
                            foreach (var p in countryPromos) Console.WriteLine($"{p.Name} (до {p.EndDate:dd.MM.yyyy})");
                            break;

                        default:
                            Console.WriteLine("Нет такого пункта.");
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
