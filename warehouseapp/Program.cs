using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WarehouseApp
{
    class Program
    {
        static IConfiguration config = null!;
        static string currentProvider = "SqlServer";
        static string connectionString = null!;

        static async Task Main(string[] args)
        {
            //2: загрузка конфига
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            currentProvider = config["DefaultProvider"] ?? "SqlServer";
            LoadConnectionString();

            // Регистрация провайдеров (Задание 1 + 6)
            DbProviderFactories.RegisterFactory("SqlServer", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Sqlite",    Microsoft.Data.Sqlite.SqliteFactory.Instance);

            while (true)
            {
                Console.WriteLine($"\nСКЛАД (провайдер: {currentProvider})");
                Console.WriteLine("1. Подключиться к БД");
                Console.WriteLine("2. Показать все товары");
                Console.WriteLine("3. Показать все типы товаров");
                Console.WriteLine("4. Показать всех поставщиков");
                Console.WriteLine("5. Добавить товар");
                Console.WriteLine("6. Удалить товар");
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
                        case "2": await ShowAllProductsAsync();
                         break;
                        case "3": await ShowAllTypesAsync();
                         break;
                        case "4": await ShowAllSuppliersAsync();
                         break;
                        case "5": await AddProductAsync();
                         break;
                        case "6": await DeleteProductAsync();
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
                    Console.WriteLine("ошибка: " + ex.Message);
                }
            }
        }

        // Задание 6: смена СУБД
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

        // Задание 1: фабрика провайдеров
        static DbConnection CreateConnection()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(currentProvider);
            DbConnection conn = factory.CreateConnection()
                ?? throw new Exception("Не удалось создать соединение.");
            conn.ConnectionString = connectionString;
            return conn;
        }

        // Задание 7: таймер
        static async Task RunWithTimerAsync(string sql, Action<DbDataReader> handler)
        {
            Stopwatch sw = Stopwatch.StartNew();

            using DbConnection conn = CreateConnection();
            await conn.OpenAsync();

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            using DbDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                handler(reader);

            sw.Stop();
            Console.WriteLine($"Запрос выполнен за {sw.Elapsed.TotalSeconds:F3} сек.");
        }

        static async Task TestConnectionAsync()
        {
            using DbConnection conn = CreateConnection();
            await conn.OpenAsync();
            Console.WriteLine("Успешное подключение к БД!");
        }

        //3: асинхронное чтение
        static async Task ShowAllProductsAsync()
        {
            string sql = @"
                SELECT p.Id, p.Name, t.Name AS TypeName, s.Name AS SupplierName,
                       p.Quantity, p.CostPrice, p.DeliveryDate
                FROM Products p
                JOIN ProductTypes t ON t.Id = p.TypeId
                JOIN Suppliers s ON s.Id = p.SupplierId";

            await RunWithTimerAsync(sql, reader =>
            {
                Console.WriteLine($"{reader["Id"]} | {reader["Name"]} | {reader["TypeName"]} | " +
                                  $"{reader["SupplierName"]} | {reader["Quantity"]} | " +
                                  $"{reader["CostPrice"]} | {reader["DeliveryDate"]}");
            });
        }

        static async Task ShowAllTypesAsync()
        {
            await RunWithTimerAsync("SELECT Id, Name FROM ProductTypes", reader =>
            {
                Console.WriteLine($"{reader["Id"]} | {reader["Name"]}");
            });
        }

        static async Task ShowAllSuppliersAsync()
        {
            await RunWithTimerAsync("SELECT Id, Name FROM Suppliers", reader =>
            {
                Console.WriteLine($"{reader["Id"]} | {reader["Name"]}");
            });
        }

        //4: асинхронное обновление
        static async Task AddProductAsync()
        {
            Console.Write("Название: ");
            string name = Console.ReadLine() ?? "";
            Console.Write("TypeId: ");
            int typeId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("SupplierId: ");
            int supplierId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Количество: ");
            int quantity = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Себестоимость: ");
            decimal cost = decimal.Parse(Console.ReadLine() ?? "0");

            using DbConnection conn = CreateConnection();
            await conn.OpenAsync();

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Products (Name, TypeId, SupplierId, Quantity, CostPrice, DeliveryDate)
                                VALUES (@Name, @TypeId, @SupplierId, @Quantity, @CostPrice, @Date)";

            AddParam(cmd, "@Name", name);
            AddParam(cmd, "@TypeId", typeId);
            AddParam(cmd, "@SupplierId", supplierId);
            AddParam(cmd, "@Quantity", quantity);
            AddParam(cmd, "@CostPrice", cost);
            AddParam(cmd, "@Date", DateTime.Now.ToString("yyyy-MM-dd"));

            int rows = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Добавлено строк: {rows}");
        }

        //5: асинхронное удаление
        static async Task DeleteProductAsync()
        {
            Console.Write("Введите Id товара для удаления: ");
            int id = int.Parse(Console.ReadLine() ?? "0");

            using DbConnection conn = CreateConnection();
            await conn.OpenAsync();

            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Products WHERE Id = @Id";
            AddParam(cmd, "@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Удалено строк: {rows}");
        }

        // Универсальное добавление параметра
        static void AddParam(DbCommand cmd, string name, object value)
        {
            DbParameter p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}
