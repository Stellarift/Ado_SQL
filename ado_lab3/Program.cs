using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ADO_Lab3
{
    class Program
    {
        static IConfiguration config = null!;
        static string cs = "";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Загрузка конфига
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            cs = config.GetConnectionString("MyLibrary")!;

            // Регистрация провайдеров
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Лабораторная №3 — ADO.NET ");
                Console.WriteLine("1. Список поставщиков данных");
                Console.WriteLine("2. Запрос через DbProviderFactory");
                Console.WriteLine("3. DataViewManager — фильтр и сортировка");
                Console.WriteLine("4. Успешная транзакция");
                Console.WriteLine("5. Транзакция с откатом");
                Console.WriteLine("0. Выход");
                Console.Write("\n> ");

                string choice = Console.ReadLine() ?? "";
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1": ShowProviders();
                         break;
                        case "2": RunQueryViaFactory();
                         break;
                        case "3": DemoDataViewManager();
                         break;
                        case "4": SuccessfulTransaction();
                         break;
                        case "5": FailedTransaction();
                         break;
                        case "0":
                         return;
                        default: Console.WriteLine("Нет такого пункта.");
                         break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ошибка: " + ex.Message);
                }

                Console.WriteLine("\nНажмите Enter...");
                Console.ReadLine();
            }
        }

        //список провайдеров
        static void ShowProviders()
        {
            DataTable providers = DbProviderFactories.GetFactoryClasses();
            Console.WriteLine("Зарегистрированные поставщики\n");
            Console.WriteLine("{0,-25} | {1}", "InvariantName", "Description");
            Console.WriteLine(new string('-', 80));
            foreach (DataRow row in providers.Rows)
            {
                string inv = row["InvariantName"].ToString()!;
                string desc = row["Description"].ToString()!;
                Console.WriteLine("{0,-25} | {1}", inv, desc);
            }
            Console.WriteLine("\nВсего найдено: " + providers.Rows.Count);
        }

        //запрос через фабрику
        static void RunQueryViaFactory()
        {
            Console.WriteLine("Выберите поставщика:");
            Console.WriteLine("  System.Data.SqlClient");
            Console.Write("> ");
            string providerName = (Console.ReadLine() ?? "").Trim();

            DbProviderFactory factory;
            try
            {
                factory = DbProviderFactories.GetFactory(providerName);
            }
            catch
            {
                Console.WriteLine("Поставщик не найден!");
                return;
            }

            Console.WriteLine("\nСтрока подключения: " + cs);

            DbConnection conn = factory.CreateConnection()!;
            conn.ConnectionString = cs;

            DbCommand cmd = factory.CreateCommand()!;
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * FROM Books";

            DbDataAdapter adapter = factory.CreateDataAdapter()!;
            adapter.SelectCommand = cmd;

            DataTable dt = new DataTable();
            try
            {
                conn.Open();
                adapter.Fill(dt);
                Console.WriteLine("\nРезультат SELECT * FROM Books\n");
                PrintDataTable(dt);
            }
            finally
            {
                conn.Close();
            }
        }

        //DataViewManager
        static void DemoDataViewManager()
        {
            DataSet set = new DataSet();
            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Authors", conn);
                adapter.Fill(set, "Authors");
            }

            Console.WriteLine("Исходные данные (без фильтра)\n");
            PrintDataTable(set.Tables["Authors"]!);

            DataViewManager dvm = new DataViewManager(set);
            dvm.DataViewSettings["Authors"]!.RowFilter = "id < 100";
            dvm.DataViewSettings["Authors"]!.Sort = "LastName ASC";

            DataView dv = dvm.CreateDataView(set.Tables["Authors"]!);

            Console.WriteLine("\nПосле фильтра (id < 100) и сортировки (LastName ASC)\n");
            PrintDataView(dv);

            Console.WriteLine("\nИсходные данные после фильтрации (не изменились)\n");
            PrintDataTable(set.Tables["Authors"]!);
        }

        //успешная транзакция
        static void SuccessfulTransaction()
        {
            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                new SqlCommand("IF OBJECT_ID('tmp3') IS NOT NULL DROP TABLE tmp3", c).ExecuteNonQuery();
            }

            SqlConnection conn = new SqlConnection(cs);
            SqlTransaction? tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();

                SqlCommand comm = conn.CreateCommand();
                comm.Transaction = tran;

                comm.CommandText = "CREATE TABLE tmp3(id INT IDENTITY PRIMARY KEY, f1 VARCHAR(20), f2 INT)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Создана таблица tmp3.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('Hello', 100)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 1.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('World', 200)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 2.");

                tran.Commit();
                Console.WriteLine("\nТРАНЗАКЦИЯ ЗАФИКСИРОВАНА");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                if (tran != null)
                {
                    tran.Rollback();
                    Console.WriteLine("ТРАНЗАКЦИЯ ОТКАЧЕНА.");
                }
            }
            finally
            {
                conn.Close();
            }

            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                int count = (int)new SqlCommand("SELECT COUNT(*) FROM tmp3", c).ExecuteScalar()!;
                Console.WriteLine("\nПроверка: строк в tmp3 = " + count + " (ожидается 2).");
            }

            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                new SqlCommand("DROP TABLE tmp3", c).ExecuteNonQuery();
                Console.WriteLine("Таблица tmp3 удалена.");
            }
        }

        //транзакция с откатом
        static void FailedTransaction()
        {
            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                new SqlCommand("IF OBJECT_ID('tmp3') IS NOT NULL DROP TABLE tmp3", c).ExecuteNonQuery();
            }

            SqlConnection conn = new SqlConnection(cs);
            SqlTransaction? tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();

                SqlCommand comm = conn.CreateCommand();
                comm.Transaction = tran;

                comm.CommandText = "CREATE TABLE tmp3(id INT IDENTITY PRIMARY KEY, f1 VARCHAR(20), f2 INT)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Создана таблица tmp3.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('Hello', 100)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 1.");

                // ОШИБКА: таблицы tmp4 не существует
                comm.CommandText = "INSERT INTO tmp4(f1, f2) VALUES('World', 200)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Эта строка НЕ должна выполниться.");

                tran.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nОШИБКА: " + ex.Message);
                if (tran != null)
                {
                    tran.Rollback();
                    Console.WriteLine("ТРАНЗАКЦИЯ ОТКАЧЕНА – все шаги отменены.");
                }
            }
            finally
            {
                conn.Close();
            }

            using (SqlConnection c = new SqlConnection(cs))
            {
                c.Open();
                int count = (int)new SqlCommand(
                    "SELECT COUNT(*) FROM sys.tables WHERE name='tmp3'", c).ExecuteScalar()!;
                Console.WriteLine("\nПроверка: таблиц tmp3 в БД = " + count + " (ожидается 0).");
            }
        }

        // Вспомогательные методы вывода
        static void PrintDataTable(DataTable dt)
        {
            if (dt.Rows.Count == 0) { Console.WriteLine("(нет данных)"); return; }
            foreach (DataColumn col in dt.Columns)
                Console.Write(col.ColumnName.PadRight(15) + "|");
            Console.WriteLine();
            Console.WriteLine(new string('-', 17 * dt.Columns.Count));
            foreach (DataRow row in dt.Rows)
            {
                foreach (var v in row.ItemArray)
                    Console.Write((v?.ToString() ?? "").PadRight(15) + "|");
                Console.WriteLine();
            }
        }

        static void PrintDataView(DataView dv)
        {
            foreach (DataColumn col in dv.Table!.Columns)
                Console.Write(col.ColumnName.PadRight(15) + "|");
            Console.WriteLine();
            Console.WriteLine(new string('-', 17 * dv.Table.Columns.Count));
            foreach (DataRowView row in dv)
            {
                foreach (var v in row.Row.ItemArray)
                    Console.Write((v?.ToString() ?? "").PadRight(15) + "|");
                Console.WriteLine();
            }
        }
    }
}