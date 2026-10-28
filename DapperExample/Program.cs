using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DapperExample
{
    // Класс-модель
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
    }

    class Program
    {
        // Строка подключения к нашей базе TestDB
        static string connectionString = "Server=localhost,1433;Database=TestDB;User Id=sa;Password=Ghoul1234;TrustServerCertificate=True;";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("1. ВСТАВКА (INSERT)");
            var newUser = new User { Name = "Иван Петров", Email = "ivan@example.com", Age = 30 };
            string insertSql = "INSERT INTO Users (Name, Email, Age) VALUES (@Name, @Email, @Age)";
            
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int rowsAffected = db.Execute(insertSql, newUser);
                Console.WriteLine($"Вставлено строк: {rowsAffected}");
            }

            Console.WriteLine("\n2. ЧТЕНИЕ (SELECT)");
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                // Получаем всех пользователей старше 25 лет
                var users = db.Query<User>("SELECT * FROM Users WHERE Age > @Age", new { Age = 25 }).ToList();
                
                foreach (var user in users)
                {
                    Console.WriteLine($"ID: {user.Id}, Имя: {user.Name}, Email: {user.Email}, Возраст: {user.Age}");
                }
            }

            Console.WriteLine("\n3. ОБНОВЛЕНИЕ (UPDATE)");
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var updateSql = "UPDATE Users SET Age = @Age WHERE Name = @Name";
                // Меняем возраст Ивана на 31
                db.Execute(updateSql, new { Age = 31, Name = "Иван Петров" });
                Console.WriteLine("Данные обновлены (Ивану теперь 31).");
            }

            // Проверка
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var updatedUser = db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Name = @Name", new { Name = "Иван Петров" });
                if (updatedUser != null) 
                    Console.WriteLine($"Проверка: {updatedUser.Name}, новый возраст: {updatedUser.Age}");
            }

            Console.WriteLine("\n4. УДАЛЕНИЕ (DELETE)");
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var deleteSql = "DELETE FROM Users WHERE Name = @Name";
                db.Execute(deleteSql, new { Name = "Иван Петров" });
                Console.WriteLine("Пользователь 'Иван Петров' удален из базы.");
            }

            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }
    }
}
