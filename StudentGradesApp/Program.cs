using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StudentGradesApp
{
    class Program
    {
        static IConfiguration config = null!;
        static string currentProvider = "SqlServer";
        static string connectionString = null!;

        static async Task Main(string[] args)
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            currentProvider = config["DefaultProvider"] ?? "SqlServer";

            DbProviderFactories.RegisterFactory("SqlServer", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Sqlite",    Microsoft.Data.Sqlite.SqliteFactory.Instance);

            LoadConnectionString();
            await InitSqliteIfNeeded();

            while (true)
            {
                Console.WriteLine($"\nОценки студентов (провайдер: {currentProvider})");
                Console.WriteLine("1. Подключиться к БД");
                Console.WriteLine("2. Показать все оценки");
                Console.WriteLine("3. Показать всех студентов");
                Console.WriteLine("4. Показать все предметы");
                Console.WriteLine("5. Добавить оценку");
                Console.WriteLine("6. Удалить оценку");
                Console.WriteLine("7. Сменить СУБД");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine() ?? "";

                try
                {
                    switch (choice)
                    {
                        case "1": await TestConnectionAsync();
                         break;
                        case "2": await ShowGradesAsync();
                         break;
                        case "3": await ShowStudentsAsync();
                         break;
                        case "4": await ShowSubjectsAsync();
                         break;
                        case "5": await AddGradeAsync();
                         break;
                        case "6": await DeleteGradeAsync();
                         break;
                        case "7": SwitchProvider();
                         break;
                        case "0": return;
                        default: Console.WriteLine("Неверный выбор");
                         break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ошибочка: " + ex.Message);
                }
            }
        }

        static void SwitchProvider()
        {
            currentProvider = currentProvider == "SqlServer" ? "Sqlite" : "SqlServer";
            LoadConnectionString();
            Console.WriteLine($"СУБД изменена на: {currentProvider}");
        }

        static void LoadConnectionString()
        {
            connectionString = config.GetConnectionString(currentProvider)
                ?? throw new Exception($"Строка подключения для {currentProvider} не найдена.");
        }

        static async Task InitSqliteIfNeeded()
        {
            if (currentProvider != "Sqlite") return;
            await using DbConnection conn = CreateConnection();
            await conn.OpenAsync();
            await using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Students (Id INTEGER PRIMARY KEY AUTOINCREMENT, FullName TEXT, GroupName TEXT);
                CREATE TABLE IF NOT EXISTS Subjects (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT);
                CREATE TABLE IF NOT EXISTS Grades (Id INTEGER PRIMARY KEY AUTOINCREMENT, StudentId INTEGER, SubjectId INTEGER, Grade INTEGER, GradeDate TEXT);";
            await cmd.ExecuteNonQueryAsync();
        }

        static DbConnection CreateConnection()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(currentProvider);
            DbConnection conn = factory.CreateConnection()!;
            conn.ConnectionString = connectionString;
            return conn;
        }

        static async Task RunWithTimerAsync(string sql, Action<DbDataReader> handler)
        {
            Stopwatch sw = Stopwatch.StartNew();

            await using DbConnection conn = CreateConnection();
            await conn.OpenAsync();

            await using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            await using DbDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                handler(reader);

            sw.Stop();
            Console.WriteLine($"\nЗапрос выполнен за {sw.Elapsed.TotalSeconds:F3} сек.");
        }

        static async Task TestConnectionAsync()
        {
            await using DbConnection conn = CreateConnection();
            await conn.OpenAsync();
            Console.WriteLine("Успешное подключение к БД!");
        }

        static async Task ShowGradesAsync()
        {
            string sql = @"SELECT g.Id, s.FullName AS StudentName, sub.Name AS SubjectName,
                                  g.Grade, g.GradeDate
                           FROM Grades g
                           JOIN Students s ON s.Id = g.StudentId
                           JOIN Subjects sub ON sub.Id = g.SubjectId";

            await RunWithTimerAsync(sql, reader =>
            {
                Console.WriteLine($"{reader["Id"]} | {reader["StudentName"]} | " +
                                  $"{reader["SubjectName"]} | {reader["Grade"]} | {reader["GradeDate"]}");
            });
        }

        static async Task ShowStudentsAsync()
        {
            await RunWithTimerAsync("SELECT Id, FullName, GroupName FROM Students", reader =>
            {
                Console.WriteLine($"{reader["Id"]} | {reader["FullName"]} | {reader["GroupName"]}");
            });
        }

        static async Task ShowSubjectsAsync()
        {
            await RunWithTimerAsync("SELECT Id, Name FROM Subjects", reader =>
            {
                Console.WriteLine($"{reader["Id"]} | {reader["Name"]}");
            });
        }

        static async Task AddGradeAsync()
        {
            Console.Write("StudentId (число): ");
            if (!int.TryParse(Console.ReadLine(), out int studentId))
            { Console.WriteLine("Должно быть число!"); return; }

            Console.Write("SubjectId (число): ");
            if (!int.TryParse(Console.ReadLine(), out int subjectId))
            { Console.WriteLine("Должно быть число!"); return; }

            Console.Write("Оценка (1-5): ");
            if (!int.TryParse(Console.ReadLine(), out int grade))
            { Console.WriteLine("Должно быть число!"); return; }

            await using DbConnection conn = CreateConnection();
            await conn.OpenAsync();

            await using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Grades (StudentId, SubjectId, Grade, GradeDate)
                                VALUES (@S, @Sub, @G, @D)";
            AddParam(cmd, "@S", studentId);
            AddParam(cmd, "@Sub", subjectId);
            AddParam(cmd, "@G", grade);
            AddParam(cmd, "@D", DateTime.Now.ToString("yyyy-MM-dd"));

            int rows = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Добавлено строк: {rows}");
        }

        static async Task DeleteGradeAsync()
        {
            Console.Write("Id оценки для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            { Console.WriteLine("Должно быть число!"); return; }

            await using DbConnection conn = CreateConnection();
            await conn.OpenAsync();

            await using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Grades WHERE Id = @Id";
            AddParam(cmd, "@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Удалено строк: {rows}");
        }

        static void AddParam(DbCommand cmd, string name, object value)
        {
            DbParameter p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}
